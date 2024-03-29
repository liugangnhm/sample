using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Stateless;

namespace AvaloniaApplication1.ViewModels;

public enum BindState
{
    NoEpc,
    Epc,
    RepeatEpc,
    Binding,
    BindingFail,
    BindOk
}

public enum BindEvent
{
    EpcEnter,
    Epc,
    BindStart,
    BindFail,
    BindOk,
    EpcExit
}

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        
        Dispatcher.UIThread.Post(() => { });
        Console.WriteLine($"current thread {Thread.CurrentThread.ManagedThreadId}");

        _machine = new StateMachine<BindState, BindEvent>(BindState.NoEpc);

        _machine.OnTransitioned(OnTransition);

        _machine.Configure(BindState.NoEpc).Permit(BindEvent.EpcEnter, BindState.Epc);

        _machine.Configure(BindState.Epc).PermitReentry(BindEvent.Epc)
            .Permit(BindEvent.EpcExit, BindState.NoEpc)
            .Permit(BindEvent.BindStart, BindState.Binding)
            ;


        _machine.Configure(BindState.Binding)
            .Permit(BindEvent.BindOk, BindState.BindOk)
            .Permit(BindEvent.BindFail, BindState.BindingFail);

        _machine.Configure(BindState.BindingFail)
            .Permit(BindEvent.EpcExit, BindState.NoEpc)
            .Permit(BindEvent.BindStart, BindState.Binding)
            .Permit(BindEvent.Epc, BindState.Epc);

        _machine.Configure(BindState.BindOk)
            .Permit(BindEvent.EpcExit, BindState.NoEpc)
            .Permit(BindEvent.Epc, BindState.Epc);
    }

    #region state machine

    private async Task FireAsync(BindEvent @event)
    {
        if (_machine.CanFire(@event))
        {
            await _machine.FireAsync(@event);
        }
    }

    private void OnTransition(StateMachine<BindState, BindEvent>.Transition transition)
    {
        Console.WriteLine($"Transitioned from {transition.Source} to " +
                          $"{transition.Destination} via {transition.Trigger}. thread {Thread.CurrentThread.ManagedThreadId}");
    }

    private readonly StateMachine<BindState, BindEvent> _machine;


    [ObservableProperty] private string? _currentEpc = "rfid001";


    [RelayCommand]
    private async Task EpcEnter()
    {
        await FireAsync(BindEvent.EpcEnter);
    }

    [RelayCommand]
    private async Task EpcRepeat()
    {
        await FireAsync(BindEvent.Epc);
    }

    [RelayCommand]
    private async Task BindStart()
    {
        await FireAsync(BindEvent.BindStart);
    }

    [RelayCommand]
    private async Task BindFail()
    {
        await FireAsync(BindEvent.BindFail);
    }

    [RelayCommand]
    private async Task BindOk()
    {
        await FireAsync(BindEvent.BindOk);
    }

    [RelayCommand]
    private async Task EpcExit()
    {
        await FireAsync(BindEvent.EpcExit);
    }

    #endregion


    [ObservableProperty] private string? _message;


    [RelayCommand]
    private async Task AsyncCommand()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Console.WriteLine("async Command executed on the UI thread");
        }

        await Task.Delay(1000);

        Message = DateTime.Now.ToString("T");
    }

    [RelayCommand]
    private void SyncCommand()
    {
        Console.WriteLine(" sync Command executed on the UI thread");
    }
}