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
using Vidora.Core.Interfaces.Storage;
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

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private Plan? _currentPlan;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasPlan;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _editFullName = string.Empty;

    [ObservableProperty]
    private string _editUsername = string.Empty;

    [ObservableProperty]
    private Gender? _editGender;

    [ObservableProperty]
    private DateTime? _editBirthday;

    [ObservableProperty]
    private string _editAvatar = string.Empty;

    [ObservableProperty]
    private BitmapImage? _previewAvatarSource;

    [ObservableProperty]
    private bool _hasNewAvatar;

    [ObservableProperty]
    private bool _isUploadingAvatar;

    private StorageFile? _selectedAvatarFile;

    public IReadOnlyList<Gender> GenderOptions { get; } = Enum.GetValues(typeof(Gender)).Cast<Gender>().ToList();

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

    public async Task OnNavigatedToAsync(object parameter)
    {
        await LoadDataAsync();
    }

    public Task OnNavigatedFromAsync()
    {
        return Task.CompletedTask;
    }

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

    [RelayCommand]
    private void StartEdit()
    {
        if (CurrentUser == null) return;

        EditFullName = CurrentUser.FullName;
        EditUsername = CurrentUser.Username;
        EditGender = CurrentUser.Gender;
        EditBirthday = CurrentUser.Birthday;
        EditAvatar = CurrentUser.Avatar ?? string.Empty;

        PreviewAvatarSource = null;
        HasNewAvatar = false;
        _selectedAvatarFile = null;

        IsEditing = true;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        EditFullName = string.Empty;
        EditUsername = string.Empty;
        EditGender = null;
        EditBirthday = null;
        EditAvatar = string.Empty;

        PreviewAvatarSource = null;
        HasNewAvatar = false;
        _selectedAvatarFile = null;

        IsEditing = false;
        ErrorMessage = string.Empty;
    }

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

    [RelayCommand]
    private void RemoveNewAvatar()
    {
        PreviewAvatarSource = null;
        HasNewAvatar = false;
        _selectedAvatarFile = null;

        if (CurrentUser != null)
        {
            EditAvatar = CurrentUser.Avatar ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveEdit()
    {
        IsSaving = true;
        ErrorMessage = string.Empty;

        try
        {
            string? avatarUrl = EditAvatar;

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

            var command = new UpdateProfileCommand
            {
                FullName = EditFullName,
                Username = EditUsername,
                Gender = EditGender,
                Birthday = EditBirthday?.ToString("yyyy-MM-dd"),
                Avatar = avatarUrl
            };

            var result = await _updateProfileUseCase.ExecuteAsync(command);

            if (result.IsSuccess)
            {
                CurrentUser = result.Value.User;

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