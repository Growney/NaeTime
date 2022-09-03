using RX5808;
using System.Device.Gpio;
using Moq;
using System.Diagnostics;
using Gpio;

namespace RX5808.Tests;

public class For_RX5808Module_ConfirmFrequency
{

    private readonly byte dataPin = 0;
    private readonly byte clockPin = 1;
    private readonly byte selectPin = 2;

    private readonly long minimumCommunicationSpacing = RX5808Module.c_communicationDelayInMicroseconds;
    private readonly long minimumTransactionSpacing = RX5808Module.c_minimumTransactionDelayInMilliseconds;

    private PinAction SetSelectPinLow(long? initialDelay = null) => PinAction.Write(initialDelay * 1000 ?? minimumCommunicationSpacing, selectPin, PinValue.Low);
    private PinAction SetSelectPinHigh(long? initialDelay = null) => PinAction.Write(initialDelay * 1000 ?? minimumCommunicationSpacing, selectPin, PinValue.High);
    private PinAction ChangeDataPinModeInput(long? initialDelay = null) => PinAction.ChangeMode(initialDelay * 1000 ?? minimumCommunicationSpacing, dataPin, PinMode.InputPullUp);
    private PinAction ChangeDataPinModeOutput(long? initialDelay = null) => PinAction.ChangeMode(initialDelay * 1000 ?? minimumCommunicationSpacing, dataPin, PinMode.Output);

    private IEnumerable<PinAction> Initialise() => new List<PinAction> { PinAction.OpenAndInitialise(selectPin, PinMode.Output, PinValue.High), PinAction.OpenAndInitialise(clockPin, PinMode.Output, PinValue.Low), PinAction.OpenAndInitialise(dataPin, PinMode.Output, PinValue.Low) };
    private IEnumerable<PinAction> WriteOne(long? initialDelay = null) => new List<PinAction> { PinAction.Write(initialDelay * 1000 ?? minimumCommunicationSpacing, dataPin, PinValue.High), PinAction.Write(minimumCommunicationSpacing, clockPin, PinValue.High), PinAction.Write(minimumCommunicationSpacing, clockPin, PinValue.Low) };
    private IEnumerable<PinAction> WriteZero(long? initialDelay = null) => new List<PinAction> { PinAction.Write(initialDelay * 1000 ?? minimumCommunicationSpacing, dataPin, PinValue.Low), PinAction.Write(minimumCommunicationSpacing, clockPin, PinValue.High), PinAction.Write(minimumCommunicationSpacing, clockPin, PinValue.Low) };
    private IEnumerable<PinAction> Read(long? initialDelay = null) => new List<PinAction> { PinAction.Write(initialDelay * 1000 ?? minimumCommunicationSpacing, clockPin, PinValue.High), PinAction.Write(minimumCommunicationSpacing, clockPin, PinValue.Low), PinAction.Read(minimumCommunicationSpacing, dataPin) };
    private IEnumerable<PinAction> ReadMultiple(int count)
    {
        var actions = new List<PinAction>();
        for (int i = 0; i < count; i++)
        {
            actions.AddRange(Read());
        }
        return actions;
    }
    private IEnumerable<PinAction> WriteMultipleOne(int count)
    {
        var actions = new List<PinAction>();
        for (int i = 0; i < count; i++)
        {
            actions.AddRange(WriteOne());
        }
        return actions;
    }
    private IEnumerable<PinAction> WriteMultipleZero(int count)
    {
        var actions = new List<PinAction>();
        for (int i = 0; i < count; i++)
        {
            actions.AddRange(WriteZero());
        }
        return actions;
    }

    private void SetupControllerMock(Mock<IGpioController> mockController, Queue<PinAction> commandQueue, params PinValue[] returns)
    {
        var stopwatch = Stopwatch.StartNew();
        mockController.Setup(x => x.OpenPin(It.IsAny<int>(), It.IsAny<PinMode>(), It.IsAny<PinValue>())).Callback<int, PinMode, PinValue>((pinNumber, mode, value) => commandQueue.Enqueue(PinAction.OpenAndInitialise(stopwatch.ElapsedTicks, pinNumber, mode, value)));
        mockController.Setup(x => x.OpenPin(It.IsAny<int>(), It.IsAny<PinMode>())).Callback<int, PinMode>((pinNumber, mode) => commandQueue.Enqueue(PinAction.Open(stopwatch.ElapsedTicks, pinNumber, mode)));
        mockController.Setup(x => x.Write(It.IsAny<int>(), It.IsAny<PinValue>())).Callback<int, PinValue>((pinNumber, value) => commandQueue.Enqueue(PinAction.Write(stopwatch.ElapsedTicks, pinNumber, value)));

        int callNumber = -1;
        mockController.Setup(x => x.Read(It.IsAny<int>())).Callback<int>(
            pinNumber =>
            {
                commandQueue.Enqueue(PinAction.Read(stopwatch.ElapsedTicks, pinNumber));
            }
           ).Returns(() =>
           {
               callNumber++;
               if (callNumber < returns.Length)
               {
                   return returns[callNumber];
               }
               else
               {
                   return PinValue.Low;
               }
           });

        mockController.Setup(x => x.SetPinMode(It.IsAny<int>(), It.IsAny<PinMode>())).Callback<int, PinMode>((pinNumber, mode) => commandQueue.Enqueue(PinAction.ChangeMode(stopwatch.ElapsedTicks, pinNumber, mode)));
    }

    [Fact]
    public async Task Test1()
    {
        var commandQueue = new Queue<PinAction>();
        var mockController = new Mock<IGpioController>(MockBehavior.Strict);
        SetupControllerMock(mockController, commandQueue);

        using var module = new RX5808.RX5808Module(mockController.Object, 0, dataPin, selectPin, clockPin);
        module.Initialise();

        var success = await module.ConfirmFrequencyAsync(0);

        var expectedActions = new List<PinAction>();
        expectedActions.AddRange(Initialise());

        expectedActions.Add(SetSelectPinLow());
        //WriteAddress
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        //Write read/right flag
        expectedActions.AddRange(WriteZero());
        //Change to read
        expectedActions.Add(ChangeDataPinModeInput());
        //ReadTheBitValues
        expectedActions.AddRange(ReadMultiple(20));
        expectedActions.Add(SetSelectPinHigh());

        AssertPinValues(commandQueue, expectedActions);

    }

    [Fact]
    public async Task Test2()
    {
        var commandQueue = new Queue<PinAction>();
        var mockController = new Mock<IGpioController>(MockBehavior.Strict);
        SetupControllerMock(mockController, commandQueue);

        using var module = new RX5808.RX5808Module(mockController.Object, 0, dataPin, selectPin, clockPin);
        module.Initialise();

        await module.SetFrequencyAsync(5800);

        var expectedActions = new List<PinAction>();
        expectedActions.AddRange(Initialise());

        expectedActions.Add(SetSelectPinLow());
        //WriteAddress
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        //Write read/right flag
        expectedActions.AddRange(WriteOne());
        //Write 0010 1001 1000 0100 LSB first
        expectedActions.AddRange(WriteMultipleZero(2));
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteMultipleZero(4));
        expectedActions.AddRange(WriteMultipleOne(2));
        expectedActions.AddRange(WriteMultipleZero(2));
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteMultipleZero(2));
        //Write the 4 bits of extra padding to make up the 20 bits
        expectedActions.AddRange(WriteMultipleZero(4));

        expectedActions.Add(SetSelectPinHigh());

        AssertPinValues(commandQueue, expectedActions);

    }

    [Fact]
    public async Task Test3()
    {
        var commandQueue = new Queue<PinAction>();
        var mockController = new Mock<IGpioController>(MockBehavior.Strict);
        SetupControllerMock(mockController, commandQueue);

        using var module = new RX5808.RX5808Module(mockController.Object, 0, dataPin, selectPin, clockPin);
        module.Initialise();

        await module.SetFrequencyAsync(5800);
        await module.ConfirmFrequencyAsync(5800);

        var expectedActions = new List<PinAction>();
        expectedActions.AddRange(Initialise());

        //Assert The Write
        expectedActions.Add(SetSelectPinLow());
        //WriteAddress
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        //Write read/right flag
        expectedActions.AddRange(WriteOne());
        //Write 0010 1001 1000 0100 LSB first
        expectedActions.AddRange(WriteMultipleZero(2));
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteMultipleZero(4));
        expectedActions.AddRange(WriteMultipleOne(2));
        expectedActions.AddRange(WriteMultipleZero(2));
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteMultipleZero(2));
        //Write the 4 bits of extra padding to make up the 20 bits
        expectedActions.AddRange(WriteMultipleZero(4));

        expectedActions.Add(SetSelectPinHigh());

        //Assert the read
        expectedActions.Add(SetSelectPinLow(minimumTransactionSpacing));
        //WriteAddress
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        //Write read/right flag
        expectedActions.AddRange(WriteZero());
        //Change to read
        expectedActions.Add(ChangeDataPinModeInput());
        //ReadTheBitValues
        expectedActions.AddRange(ReadMultiple(20));
        expectedActions.Add(SetSelectPinHigh());

        AssertPinValues(commandQueue, expectedActions);

    }

    [Fact]
    public async Task Test4()
    {
        var commandQueue = new Queue<PinAction>();
        var mockController = new Mock<IGpioController>(MockBehavior.Strict);
        //Write 0010 1001 1000 0100 LSB first
        SetupControllerMock(mockController, commandQueue,
            PinValue.Low,
            PinValue.Low,
            PinValue.High,
            PinValue.Low,
            PinValue.Low,
            PinValue.Low,
            PinValue.Low,
            PinValue.High,
            PinValue.High,
            PinValue.Low,
            PinValue.Low,
            PinValue.High,
            PinValue.Low,
            PinValue.High);

        using var module = new RX5808.RX5808Module(mockController.Object, 0, dataPin, selectPin, clockPin);
        module.Initialise();

        var success = await module.ConfirmFrequencyAsync(5800);

        var expectedActions = new List<PinAction>();
        expectedActions.AddRange(Initialise());

        expectedActions.Add(SetSelectPinLow());
        //WriteAddress
        expectedActions.AddRange(WriteOne());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        expectedActions.AddRange(WriteZero());
        //Write read/right flag
        expectedActions.AddRange(WriteZero());
        //Change to read
        expectedActions.Add(ChangeDataPinModeInput());
        //ReadTheBitValues
        expectedActions.AddRange(ReadMultiple(20));
        expectedActions.Add(SetSelectPinHigh());

        Assert.True(success);

        AssertPinValues(commandQueue, expectedActions);

    }
    private void AssertPinValues(Queue<PinAction> actual, IEnumerable<PinAction> expected)
    {
        PinAction? lastAction = null;
        foreach (var expectedAction in expected)
        {
            var actualAction = actual.Dequeue();
            if (lastAction != null)
            {
                AssertMinimumMicroseconds(expectedAction.OnTick, lastAction.Value, actualAction);
            }

            lastAction = actualAction;

            Assert.Equal(expectedAction.Type, actualAction.Type);

            switch (expectedAction.Type)
            {
                case PinActionType.Read:
                    Assert.Equal(expectedAction.PinNumber, actualAction.PinNumber);
                    break;
                case PinActionType.Write:
                    Assert.Equal(expectedAction.PinNumber, actualAction.PinNumber);
                    Assert.Equal(expectedAction.Value, actualAction.Value);
                    break;
                case PinActionType.ChangeMode:
                    Assert.Equal(expectedAction.PinNumber, actualAction.PinNumber);
                    Assert.Equal(expectedAction.Mode, actualAction.Mode);
                    break;
                default:
                    break;
            }
        }
        Assert.Empty(actual);
    }
    private void AssertMinimumMicroseconds(long minimumMicroseconds, PinAction previousAction, PinAction currentAction)
    {
        var ticksPerSecond = Stopwatch.Frequency;
        var ticksPerMicrosecond = ticksPerSecond / 1000000;

        var tickPeriod = currentAction.OnTick - previousAction.OnTick;
        var minimumTick = ticksPerMicrosecond * minimumMicroseconds;

        Assert.True(tickPeriod > minimumTick, $"Incorrect spacing minimum required: {minimumTick} ticks actual: {tickPeriod} ticks between previous: {previousAction} and current: {currentAction}");

    }
}