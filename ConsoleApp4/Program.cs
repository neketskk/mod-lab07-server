
using System;
using System.Threading;

namespace L7
{
    public class Prin : EventArgs
    {
        public int log
        {
            get;
            set;
        }
    }
    public class Server
    {
        public int appeal = 0;
        public int treated = 0;
        public int outlaw = 0;
        public static int n;
        static Pool[] pool;
        struct Pool
        {

            public bool use;
            public int count;
            public int wait;
            public Thread thread;
        }
        public Server(int val)
        {
            n = val;
            pool = new Pool[n];
            for (int i = 0; i < n; ++i)
            {
                pool[i].use = false;
                pool[i].count = 0;
                pool[i].wait = 0;
            }
        }
        public static void Answer(object data)
        {
            Thread.Sleep(10);
            Console.WriteLine("Request number: {0} served", data);
            for (int i = 0; i < n; i++)
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].use = false;
                    break;
                }
        }



        public int Get_W(int n)
        {
            return pool[n].wait;
        }
        public int Get_C(int n)
        {
            return pool[n].count;
        }

        object threadLock = new object();
        public void proc(object sender, Prin e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Application with a number: {0}", e.log);
                appeal++;
                for (int i = 0; i < n; ++i)
                {
                    if (pool[i].use == false)
                    {
                        pool[i].wait++;
                    }
                }
                for (int i = 0; i < n; i++)
                {
                    if (!pool[i].use)
                    {
                        pool[i].count++;
                        pool[i].use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(e.log);
                        treated++;
                        return;
                    }
                }
                outlaw++;
            }
        }
    }
    public class Client
    {
        public Server server;
        public event EventHandler<Prin> request;
        public Client(Server server)
        {
            this.server = server;
            this.request += server.proc;
        }
        protected virtual void OnProc(Prin e)
        {
            request?.Invoke(this, e);
        }
        public void sendRequest(int log)
        {
            Prin ev = new Prin();
            ev.log = log;
            OnProc(ev);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("======================================================\n");
            long fun(int n)
            {
                long x = 1;
                for (int i = 1; i <= n; i++)
                    x *= i;
                return x;
            }
            int clIntens = 50;
            int svIntens = 5;
            Server server = new Server(svIntens);
            Client[] client = new Client[50];
            for (int i = 0; i < clIntens; i++)
            {
                client[i] = new Client(server);
                client[i].sendRequest(i + 1);
            }

            Thread.Sleep(200);
            Console.WriteLine($"In total : {server.appeal}. Done: {server.treated}. Rejected: {server.outlaw}");

            Console.WriteLine("======================================================\n");
            for (int i = 0; i < svIntens; ++i)
            {
                Console.WriteLine($"Flow {i + 1} done {server.Get_C(i)} applications. Downtime {server.Get_W(i)} ");
            }



            double p = (double)clIntens / svIntens;
            double P_n = 0;
            for (int i = 0; i < svIntens; ++i)
            {
                P_n += Math.Pow(p, i) / fun(i);
            }
            P_n = Math.Pow(P_n, -1);
            double Pn = (double)server.outlaw / clIntens;
            double FF = 1 - Pn;
            double A = clIntens * FF;
            double k = A / svIntens;
            Console.WriteLine("======================================================\n");
            Console.WriteLine($"Reduced application flow rate: {p}");
            Console.WriteLine($"Probability of system downtime: {P_n}");
            Console.WriteLine($"System failure probability: {Pn}");
            Console.WriteLine($"Average number of busy channels: {k}");
            Console.WriteLine($"Relative throughput: {FF}");
            Console.WriteLine($"Absolute throughput: {A}");
            Console.WriteLine("======================================================\n");

            Console.ReadKey();
        }
    }
}