using System.Reflection;
using AwesomeAssertions;
using UnionGen.Test.Infrastructure;

namespace UnionGen.Test;

/// <summary>
/// Compiles real unions together with the generator, loads the emitted assembly and exercises the
/// generated members through a hand-written <c>Driver</c> so we assert actual runtime behaviour.
/// </summary>
public sealed class BehavioralTests
{
    private const string Source = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Threading.Tasks;
                                  using UnionGen;

                                  namespace Behavior;

                                  [Union<int, string>]
                                  public readonly partial struct IntOrString;

                                  [Union<int, double, long>]
                                  public readonly partial struct Numbers;

                                  [Union<string, Exception>]
                                  public readonly partial struct StringOrException;

                                  [Union<int, long[]>]
                                  public readonly partial struct IntOrLongArray;

                                  public class Animal
                                  {
                                      public override bool Equals(object? obj) => ReferenceEquals(this, obj);
                                      public override int GetHashCode() => 1;
                                  }

                                  public sealed class Dog : Animal;

                                  [Union<Animal, Dog>]
                                  public readonly partial struct AnimalOrDog;

                                  public static class Driver
                                  {
                                      public static bool ValueCase_IsAndAs()
                                      {
                                          IntOrString u = 5;
                                          return u.IsInt && !u.IsString && u.AsInt() == 5;
                                      }

                                      public static bool RefCase_IsAndAs()
                                      {
                                          IntOrString u = "hello";
                                          return u.IsString && !u.IsInt && u.AsString() == "hello";
                                      }

                                      public static bool As_WrongType_Throws()
                                      {
                                          IntOrString u = 5;
                                          try { u.AsString(); return false; }
                                          catch (InvalidOperationException) { return true; }
                                      }

                                      public static bool Implicit_Operator()
                                      {
                                          IntOrString u = 42;
                                          return u.IsInt && u.AsInt() == 42;
                                      }

                                      public static int Match_DispatchesToCorrectCase()
                                      {
                                          Numbers u = 3L;
                                          return u.Match(_ => 1, _ => 2, _ => 3);
                                      }

                                      public static int Switch_DispatchesToCorrectCase()
                                      {
                                          Numbers u = 2.5;
                                          var hit = -1;
                                          u.Switch(_ => hit = 0, _ => hit = 1, _ => hit = 2);
                                          return hit;
                                      }

                                      public static int MatchAsync_Awaits() => MatchAsyncInner().GetAwaiter().GetResult();

                                      private static async ValueTask<int> MatchAsyncInner()
                                      {
                                          IntOrString u = 10;
                                          return await u.MatchAsync(async i => { await Task.Yield(); return i + 1; },
                                                                    _ => ValueTask.FromResult(-1));
                                      }

                                      public static int SwitchAsync_Awaits() => SwitchAsyncInner().GetAwaiter().GetResult();

                                      private static async ValueTask<int> SwitchAsyncInner()
                                      {
                                          IntOrString u = "abc";
                                          var hit = -1;
                                          await u.SwitchAsync(_ => ValueTask.CompletedTask,
                                                              async s => { await Task.Yield(); hit = s.Length; });
                                          return hit;
                                      }

                                      public static string ToString_Value()
                                      {
                                          IntOrString u = 7;
                                          return u.ToString();
                                      }

                                      public static string ToString_NullRef()
                                      {
                                          StringOrException u = (string)null!;
                                          return u.ToString();
                                      }

                                      public static bool Array_Case()
                                      {
                                          long[] data = [1L, 2L, 3L];
                                          IntOrLongArray u = data;
                                          return u.IsLongArray && u.AsLongArray().Length == 3;
                                      }

                                      public static bool Equality_SameValueCase_Equal()
                                      {
                                          IntOrString a = 5;
                                          IntOrString b = 5;
                                          return a.Equals(b) && a == b && a.GetHashCode() == b.GetHashCode();
                                      }

                                      public static bool Equality_DifferentValue_NotEqual()
                                      {
                                          IntOrString a = 5;
                                          IntOrString b = 6;
                                          return !a.Equals(b) && a != b;
                                      }

                                      public static bool Equality_DifferentCase_NotEqual()
                                      {
                                          IntOrString a = 5;
                                          IntOrString b = "5";
                                          return !a.Equals(b) && a != b;
                                      }

                                      public static bool Equality_SameRefCase_Equal()
                                      {
                                          var s = "shared";
                                          IntOrString a = s;
                                          IntOrString b = s;
                                          return a.Equals(b) && a == b && a.GetHashCode() == b.GetHashCode();
                                      }

                                      public static bool Equality_NullRefSameCase_Equal()
                                      {
                                          StringOrException a = (string)null!;
                                          StringOrException b = (string)null!;
                                          return a.Equals(b) && a == b;
                                      }

                                      // Regression for the discriminator bug: the *same* object held as two different
                                      // overlapping cases must compare as distinct, consistent with Is*/Switch/Match.
                                      public static bool Equality_OverlappingRefCases_Distinct()
                                      {
                                          var dog = new Dog();
                                          var asAnimal = new AnimalOrDog((Animal)dog);
                                          var asDog = new AnimalOrDog(dog);
                                          return asAnimal.IsAnimal
                                                 && asDog.IsDog
                                                 && !asAnimal.Equals(asDog)
                                                 && asAnimal != asDog;
                                      }
                                  }
                                  """;

    private static readonly Assembly Assembly = GeneratorTestHelper.CompileAndLoad(Source);

    private static bool Bool(string method) => (bool) Assembly.InvokeStatic("Behavior.Driver", method)!;
    private static int Int(string method) => (int) Assembly.InvokeStatic("Behavior.Driver", method)!;
    private static string Str(string method) => (string) Assembly.InvokeStatic("Behavior.Driver", method)!;

    [Fact]
    public void ValueCase_IsAndAs_Work() => Bool("ValueCase_IsAndAs").Should().BeTrue();

    [Fact]
    public void ReferenceCase_IsAndAs_Work() => Bool("RefCase_IsAndAs").Should().BeTrue();

    [Fact]
    public void As_WrongType_Throws() => Bool("As_WrongType_Throws").Should().BeTrue();

    [Fact]
    public void Implicit_Operator_Works() => Bool("Implicit_Operator").Should().BeTrue();

    [Fact]
    public void Match_DispatchesToCorrectCase() => Int("Match_DispatchesToCorrectCase").Should().Be(3);

    [Fact]
    public void Switch_DispatchesToCorrectCase() => Int("Switch_DispatchesToCorrectCase").Should().Be(1);

    [Fact]
    public void MatchAsync_Awaits() => Int("MatchAsync_Awaits").Should().Be(11);

    [Fact]
    public void SwitchAsync_Awaits() => Int("SwitchAsync_Awaits").Should().Be(3);

    [Fact]
    public void ToString_Value() => Str("ToString_Value").Should().Be("7");

    [Fact]
    public void ToString_NullRef_ReturnsNullString() => Str("ToString_NullRef").Should().Be("null");

    [Fact]
    public void Array_Case_Works() => Bool("Array_Case").Should().BeTrue();

    [Fact]
    public void Equality_SameValueCase_Equal() => Bool("Equality_SameValueCase_Equal").Should().BeTrue();

    [Fact]
    public void Equality_DifferentValue_NotEqual() => Bool("Equality_DifferentValue_NotEqual").Should().BeTrue();

    [Fact]
    public void Equality_DifferentCase_NotEqual() => Bool("Equality_DifferentCase_NotEqual").Should().BeTrue();

    [Fact]
    public void Equality_SameRefCase_Equal() => Bool("Equality_SameRefCase_Equal").Should().BeTrue();

    [Fact]
    public void Equality_NullRefSameCase_Equal() => Bool("Equality_NullRefSameCase_Equal").Should().BeTrue();

    [Fact]
    public void Equality_OverlappingRefCases_Distinct() => Bool("Equality_OverlappingRefCases_Distinct").Should().BeTrue();
}
