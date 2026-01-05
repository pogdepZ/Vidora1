using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vidora.Core.Contracts.Commands;
using Vidora.Core.Entities;
using Vidora.Core.Interfaces.Services;
using Vidora.Core.UseCases;
using Vidora.Presentation.Gui.Contracts.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Vidora.Presentation.Gui.ViewModels;

public partial class ProfileViewModel : ObservableObject, INavigationAware
{
    private readonly GetProfileUseCase _getProfileUseCase;
    private readonly UpdateProfileUseCase _updateProfileUseCase;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMapper _mapper;

    // --- CÁC BIẾN HIỂN THỊ (VIEW MODE) ---
    [ObservableProperty]
    private User _currentUser;

    [ObservableProperty]
    private Plan _currentPlan;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasPlan;

    [ObservableProperty]
    private string _errorMessage;

    // --- CÁC BIẾN CHỈNH SỬA (EDIT MODE) - Observable riêng để binding hoạt động ---
    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _editFullName;

    [ObservableProperty]
    private string _editUsername;

    [ObservableProperty]
    private Gender? _editGender;

    [ObservableProperty]
    private DateTime? _editBirthday;

    [ObservableProperty]
    private string _editAvatar;

    // --- AVATAR PREVIEW ---
    [ObservableProperty]
    private BitmapImage _previewAvatarSource;

    [ObservableProperty]
    private bool _hasNewAvatar;

    [ObservableProperty]
    private bool _isUploadingAvatar;

    private StorageFile _selectedAvatarFile;

    public IReadOnlyList<Gender> GenderOptions { get; } = Enum.GetValues(typeof(Gender)).Cast<Gender>().ToList();

    // Constructor
    public ProfileViewModel(
        GetProfileUseCase getProfileUseCase,
        UpdateProfileUseCase updateProfileUseCase,
        ICloudinaryService cloudinaryService,
        IMapper mapper)
    {
        _getProfileUseCase = getProfileUseCase;
        _updateProfileUseCase = updateProfileUseCase;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
    }

    // --- NAVIGATION ---
    public async Task OnNavigatedToAsync(object parameter)
    {
        await LoadDataAsync();
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

    // --- LOAD DATA ---
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _getProfileUseCase.ExecuteAsync();

            if (result.IsSuccess)
            {
                var data = result.Value;

                if (string.IsNullOrEmpty(data.User.Avatar))
                {
                    data.User.Avatar = "ms-appx:///Assets/default-avatar.png";
                }

                CurrentUser = data.User;
                CurrentPlan = data.CurrentPlan;
                HasPlan = data.CurrentPlan != null;
            }
            else
            {
                ErrorMessage = result.Error;
                Debug.WriteLine($"Lỗi lấy profile: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Đã xảy ra lỗi không mong muốn.";
            Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- COMMANDS CHỈNH SỬA ---

    // 1. Bắt đầu sửa: Copy dữ liệu sang các property Edit riêng
    [RelayCommand]
    private void StartEdit()
    {
        if (CurrentUser == null) return;

        // Copy dữ liệu sang các property observable riêng
        EditFullName = CurrentUser.FullName;
        EditUsername = CurrentUser.Username;
        EditGender = CurrentUser.Gender;
        EditBirthday = CurrentUser.Birthday;
        EditAvatar = CurrentUser.Avatar;

        // Reset avatar preview
        PreviewAvatarSource = null;
        HasNewAvatar = false;
        _selectedAvatarFile = null;

        IsEditing = true;
        ErrorMessage = string.Empty;
    }

    // 2. Hủy bỏ: Reset các property Edit
    [RelayCommand]
    private void CancelEdit()
    {
        EditFullName = null;
        EditUsername = null;
        EditGender = null;
        EditBirthday = null;
        EditAvatar = null;

        PreviewAvatarSource = null;
        HasNewAvatar = false;
        _selectedAvatarFile = null;

        IsEditing = false;
        ErrorMessage = string.Empty;
    }

    // 3. Chọn ảnh đại diện (avatar) từ bộ lựa chọn file
    [RelayCommand]
    private async Task PickAvatarAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".webp");

            // Get the current window handle for WinUI 3
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                _selectedAvatarFile = file;

                // Load image for preview
                using var stream = await file.OpenReadAsync();
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);
                PreviewAvatarSource = bitmap;
                HasNewAvatar = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Không thể chọn ảnh. Vui lòng thử lại.";
            Debug.WriteLine($"Error picking avatar: {ex}");
        }
    }

    // 4. Xóa ảnh đại diện mới đã chọn, trở về ảnh cũ nếu có
    [RelayCommand]
    private void RemoveNewAvatar()
    {
        PreviewAvatarSource = null;
        HasNewAvatar = false;
        _selectedAvatarFile = null;

        if (CurrentUser != null)
        {
            EditAvatar = CurrentUser.Avatar;
        }
    }

    // 5. Lưu thay đổi: Gọi API Update
    [RelayCommand]
    private async Task SaveEdit()
    {
        IsSaving = true;
        ErrorMessage = string.Empty;

        try
        {
            string? avatarUrl = EditAvatar;

            // Upload ảnh mới lên Cloudinary nếu có
            if (HasNewAvatar && _selectedAvatarFile != null)
            {
                IsUploadingAvatar = true;

                using var stream = await _selectedAvatarFile.OpenStreamForReadAsync();
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    stream,
                    _selectedAvatarFile.Name,
                    "vidora/avatars"
                );

                IsUploadingAvatar = false;

                if (uploadResult.IsFailure)
                {
                    ErrorMessage = $"Không thể upload ảnh: {uploadResult.Error}";
                    return;
                }

                avatarUrl = uploadResult.Value;
            }

            // Tạo Command từ các property Edit
            var command = new UpdateProfileCommand
            {
                FullName = EditFullName,
                Username = EditUsername,
                Gender = EditGender,
                Birthday = EditBirthday?.ToString("yyyy-MM-dd"),
                Avatar = avatarUrl
            };

            // Gọi UseCase
            var result = await _updateProfileUseCase.ExecuteAsync(command);

            // Xử lý kết quả
            if (result.IsSuccess)
            {
                // Thành công: Cập nhật User gốc bằng dữ liệu mới nhất từ Server
                CurrentUser = result.Value.User;

                // Reset edit state
                CancelEdit();
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi khi lưu thông tin.";
            Debug.WriteLine(ex);
        }
        finally
        {
            IsSaving = false;
            IsUploadingAvatar = false;
        }
    }
}