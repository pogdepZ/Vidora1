using System.Collections.ObjectModel;

namespace Vidora.Presentation.Gui.Models;

public class GenreMovieGroup
{
    public string GenreName { get; set; } = string.Empty;
    public ObservableCollection<Movie> Movies { get; set; } = [];
}
