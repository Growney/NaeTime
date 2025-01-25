﻿using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Pages.HardwarePages;
public partial class UpdateSerialEsp32NodeTimer
{
    [Inject]
    private IRemoteProcedureCallClient RpcClient { get; set; } = null!;
    [Inject]
    private IEventClient EventClient { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid TimerId { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    private SerialEsp32Node? _model = null;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnInitializedAsync();

        Hardware.Messages.Models.SerialEsp32Node? response = await RpcClient.InvokeAsync<Hardware.Messages.Models.SerialEsp32Node?>("GetSerialEsp32NodeTimer", TimerId);

        if (response == null)
        {
            return;
        }

        _model = new SerialEsp32Node
        {
            Id = response.TimerId,
            Name = response.Name,
            Port = response.Port
        };
    }

    private async Task HandleValidSubmit(SerialEsp32Node timer)
    {
        if (_model is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(timer.Name))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(timer.Port))
        {
            return;
        }


        await EventClient.PublishAsync(new SerialEsp32NodeConfigured(timer.Id, timer.Name, timer.Port));

        NavigationManager.NavigateTo(ReturnUrl ?? "/hardware/list");
    }
}
