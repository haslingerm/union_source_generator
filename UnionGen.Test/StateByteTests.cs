using AwesomeAssertions;
using UnionGen.InternalUtil;

namespace UnionGen.Test;

public sealed class StateByteTests
{
    private const int MinValue = 0;
    private const int MaxValue = 15;

    [Fact]
    public void RefIndex_SetToMax()
    {
        StateByte.RefTypeIndex.Should().Be(MaxValue);
    }
    
    [Fact]
    public void Index_SetAndGet_Correctly()
    {
        // due to the limit range we can test all values
        for (var i = MinValue; i <= MaxValue; i++)
        {
            var stateByte = new StateByte(i, 0);
            stateByte.Index.Should().Be(i);
        }
    }

    [Fact]
    public void Index_Set_RangeChecked()
    {
        Action act1 = () => new StateByte(MaxValue + 1, 0);
        Action act2 = () => new StateByte(MinValue - 1, 0);
        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void ActualTypeIndex_SetAndGet_Correctly()
    {
        // due to the limit range we can test all values
        for (var i = MinValue; i <= MaxValue; i++)
        {
            var stateByte = new StateByte(0, i);
            stateByte.ActualTypeIndex.Should().Be(i);
        }
    }
    
    [Fact]
    public void ActualTypeIndex_Set_RangeChecked()
    {
        Action act1 = () => new StateByte(0, MaxValue + 1);
        Action act2 = () => new StateByte(0, MinValue - 1);
        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void Index_And_ActualTypeIndex_Independent_Correctly()
    {
        const int Index = 3;
        const int ActualTypeIndex = 5;
        
        var stateByte = new StateByte(Index, ActualTypeIndex);
        stateByte.Index.Should().Be(Index);
        stateByte.ActualTypeIndex.Should().Be(ActualTypeIndex);
    }
}
