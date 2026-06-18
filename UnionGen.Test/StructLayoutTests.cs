using System.Runtime.InteropServices;
using AwesomeAssertions;

namespace UnionGen.Test;

public sealed class StructLayoutTests
{
    [Fact]
    public void Values_Correct()
    {
        const int State = 4;
        const int IntValue = 3;
        const double DoubleValue = 3.14;
        var obj = new object();
        
        var layout1 = new FixedLayout(State, IntValue);
        var layout2 = new FixedLayout(State, DoubleValue);
        var layout3 = new FixedLayout(State, obj);
        
        layout1.stateByte.Should().Be(State);
        layout1.value0.Should().Be(IntValue);
        layout2.stateByte.Should().Be(State);
        layout2.value1.Should().Be(DoubleValue);
        layout3.stateByte.Should().Be(State);
        layout3.valueRef.Should().Be(obj);
    }
    
    [StructLayout(LayoutKind.Explicit)]
    private readonly struct FixedLayout
    {
        [FieldOffset(8)]
        public readonly byte stateByte;
        [FieldOffset(0)]
        public readonly object? valueRef;
        [FieldOffset(9)]
        public readonly int value0;
        [FieldOffset(9)]
        public readonly double value1;
        
        public FixedLayout(byte stateByte, object? valueRef)
        {
            this.stateByte = stateByte;
            this.valueRef = valueRef;
        }
        
        public FixedLayout(byte stateByte, int value)
        {
            this.stateByte = stateByte;
            this.value0 = value;
        }
        
        public FixedLayout(byte stateByte, double value)
        {
            this.stateByte = stateByte;
            this.value1 = value;
        }
    }
}
