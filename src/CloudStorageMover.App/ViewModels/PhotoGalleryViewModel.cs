using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudStorageMover.App.ViewModels;

public partial class PhotoGalleryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Photo Gallery";

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<PhotoGroup> Groups { get; } = new();

    public void LoadFromFolder(string folderPath)
    {
        Groups.Clear();
        IsLoading = true;

        try
        {
            if (!Directory.Exists(folderPath))
                return;

            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".heic", ".heif"
            };

            var files = Directory.GetFiles(folderPath)
                .Where(f => extensions.Contains(Path.GetExtension(f)))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            if (files.Count == 0)
                return;

            var now = DateTime.Now;
            var oneWeekAgo = now.AddDays(-7);
            var oneMonthAgo = now.AddMonths(-1);

            var oldestDate = files.Last().LastWriteTime;

            bool useDayGrouping = oldestDate >= oneWeekAgo;
            bool useWeekGrouping = !useDayGrouping && oldestDate >= oneMonthAgo;

            IEnumerable<IGrouping<string, FileInfo>> grouped;
            if (useDayGrouping)
            {
                grouped = files.GroupBy(f => f.LastWriteTime.ToString("dddd, MMMM d, yyyy"));
            }
            else if (useWeekGrouping)
            {
                grouped = files.GroupBy(f =>
                {
                    var weekStart = f.LastWriteTime.Date.AddDays(-(int)f.LastWriteTime.DayOfWeek);
                    var weekEnd = weekStart.AddDays(6);
                    return $"Week of {weekStart:MMM d} - {weekEnd:MMM d, yyyy}";
                });
            }
            else
            {
                grouped = files.GroupBy(f => f.LastWriteTime.ToString("MMMM yyyy"));
            }

            foreach (var group in grouped)
            {
                var photoGroup = new PhotoGroup { Title = group.Key };
                foreach (var file in group)
                {
                    photoGroup.Photos.Add(new PhotoItem
                    {
                        FilePath = file.FullName,
                        FileName = file.Name,
                        DateTaken = file.LastWriteTime
                    });
                }
                Groups.Add(photoGroup);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class PhotoGroup
{
    public string Title { get; set; } = string.Empty;
    public ObservableCollection<PhotoItem> Photos { get; } = new();
}

public class PhotoItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime DateTaken { get; set; }
}
