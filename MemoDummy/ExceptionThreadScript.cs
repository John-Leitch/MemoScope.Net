using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace MemoDummy
{
    public class ExceptionThreadScript : AbstractMemoScript
    {
        public override string Name => "Exception Threads";
        public override string Description => "Creates some threads throwing exceptions";

        [Category("Config")]
        public long NbThreads { get; set; } = 4;

        public override void Run()
        {
            for (int i = 0; i < NbThreads; i++)
            {
                Thread thread = new Thread(() =>
                {
                    int[] arrayInt = new int[5];
                    try
                    {
                        int x = arrayInt[arrayInt.Length];
                        if (x < 0)
                        {
                            Debug.WriteLine("Huh ?");
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Something failed !", e);
                    }
                })
                {
                    Name = "thread #" + i
                };
                thread.Start();
            }
        }
    }
}
