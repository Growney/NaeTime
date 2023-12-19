namespace NaeTime.Client.Razor.Abstractions;
public interface INavigationManager
{
    void NavigateTo(string uri);
    void GoBack();
}
