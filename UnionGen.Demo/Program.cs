using UnionDemo;
using UnionGen;
using UnionGen.Types;
using UnionTest;

var demo = new DemoObj(4);
Console.WriteLine(demo.IsInt);
Console.WriteLine(demo.AsInt());
await demo.SwitchAsync(async i =>
                       {
                           await Task.Delay(100);
                           Console.WriteLine(i);
                       },
                       _ => ValueTask.CompletedTask,
                       _ => ValueTask.CompletedTask,
                       _ => ValueTask.CompletedTask,
                       _ => ValueTask.CompletedTask);
var matchRes = await demo.MatchAsync(async i =>
                                     {
                                         await Task.Delay(100);

                                         return i * i;
                                     },
                                     _ => ValueTask.FromResult(-1),
                                     _ => ValueTask.FromResult(-2),
                                     _ => ValueTask.FromResult(-3),
                                     _ => ValueTask.FromResult(-4));
Console.WriteLine(matchRes);

var demo2 = new DemoObj([1L, 2L, 3L]);
Console.WriteLine(demo2.IsLongArray);
Console.WriteLine(demo2.IsListOfFoo);
demo2.Switch(_ => { },
             _ => { },
             a => Console.WriteLine($"{a.GetType()} {a.Length}"),
             _ => { },
             _ => { });

var simple = new SimpleObj(new Result<int>(12));
var notFound = simple.IsNotFound;
var result = simple.AsResultOfInt();
Console.WriteLine(result);

simple.Switch(r => Console.WriteLine($"Found: {r}"),
              _ => Console.WriteLine("not found"));
var res = simple.Match(r => r.Value * 2,
                       _ => -1);

var nested = new INested.Nested(42);

Console.WriteLine(GetOneOfType());

return;

SimpleObj CreateSimple() => new NotFound();

Union<Result<string>, NotFound> GetOneOfType() => new Result<string>("hello");
