using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Abstractions;

namespace NaeTime.Client.Razor.Lib;
public class StackedNavigationManager : INavigationManager
{
    private readonly Stack<string> _paths = new();
    private NavigationManager _navigationManager;

    public StackedNavigationManager(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    public void GoBack()
    {
        string destination;
        if (_paths.Count > 1)
        {
            _paths.Pop(); //pop the current page
            destination = _paths.Pop();
        }
        else
        {
            destination = "/";
        }
        NavigateTo(destination);
    }

    public void NavigateTo(string uri)
    {
        _paths.Push(uri);
        _navigationManager.NavigateTo(uri);
    }
}
