using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace sai3.imitation
{
    public class SMP
    {
        struct State2
        {
            private int period;
            private Random random;
            private int queueMax;
            private double p1, p2;

            public int Time { get; private set; }
            public int Queue { get => queue.Count(); }
            public bool Proc1 { get; private set; }
            public bool Proc2 { get; private set; }

            public bool Proc1Worked { get; private set; }
            public bool Proc2Worked { get; private set; }
            public int Refusal { get; private set; }
            public int InSystem { get { return Queue + (Proc1 ? 1 : 0) + (Proc2 ? 1 : 0); } }
            public int timeInQue;
            public int timeInSystem;
            public int NC, NQ;
            public float wq { get => (timeInQue / (float)NQ); }
            public float wc { get => (timeInSystem / (float)NC); }

            public State2(float p1, float p2, int period, int queueMax, Random random)
            {
                this.period = period;
                this.p1 = p1;
                this.p2 = p2;
                this.queueMax = queueMax;
                this.random = random;

                Time = 2;
                Proc1 = false;
                Proc2 = false;

                NC = NQ = 0;
                timeInQue = 0;
                timeInSystem = 0;
                Proc1Worked = Proc2Worked = false;
                Refusal = 0;

                queue = new Queue<int>();
            }

            Queue<int> queue;
            int proc1Time = 0, proc2Time = 0;
            public void tick()
            {
                
                Time = period - (Time + 1) % period;
                bool gen = false;
                if (Time == 2)
                {
                    gen=true;
                    queue.Enqueue(0);
                }
                
                CalcProc1();
                CalcProc2();
                if (queue.Count<=queueMax)
                {
                    var newQ = new Queue<int>();
                    while(queue.Count>0)
                    {
                        newQ.Enqueue(queue.Dequeue() + 1);
                    }
                    queue = newQ;
                }
                else
                {
                    Refusal = gen ? 1 : 0;
                    var newQ =new Queue<int>();
                    for (int i = 0; i < queueMax; i++)
                    {
                        newQ.Enqueue( queue.Dequeue()+1);
                    }
                    queue = newQ;
                }
            }

            public void CalcProc1()
            {
                bool worked1 = random.NextDouble() > p1;
                Proc1Worked = Proc1 && worked1;
                if (queue.Count > 0)
                {
                    if (!Proc1) // if empty 
                    {
                        proc1Time = queue.Dequeue();
                        timeInQue += proc1Time;
                        NQ++;
                        Proc1 = true;
                    }
                    else if (Proc1 && worked1)
                    {
                        NC++;
                        timeInSystem += proc1Time;

                        proc1Time = queue.Dequeue();
                        timeInQue += proc1Time;
                        NQ++;
                    }

                }
                else if (Proc1 && worked1)
                {
                    NC++;
                    timeInSystem += proc1Time;
                    proc1Time = 0;
                    Proc1 = false;
                }

                if (Proc1)
                {
                    proc1Time++;
                }
            }


            public void CalcProc2()
            {
                bool worked2 = random.NextDouble() > p2;
                Proc2Worked = Proc2 && worked2;
                if (queue.Count > 0)
                {
                    if (!Proc2) // if empty 
                    {
                        proc2Time = queue.Dequeue();
                        timeInQue += proc2Time;
                        NQ++;
                        Proc2 = true;
                    }
                    else if (Proc2 && worked2) // if worked
                    {
                        NC++;
                        timeInSystem += proc2Time;

                        proc2Time = queue.Dequeue();
                        timeInQue += proc2Time;
                        NQ++;
                    }

                }
                else if (Proc2 && worked2)//empty
                {
                    NC++;
                    timeInSystem += proc2Time;
                    proc2Time = 0;
                    Proc2 = false;
                }

                if (Proc2)
                {
                    proc2Time++;
                }
            }

            public String CurrentState()
            {

                return $"{Time}{queue.Count}{(Proc1 ? 1 : 0)}{(Proc2 ? 1 : 0)}";
            }
        }

        State2 stateMachine;
        public SMP(float p1, float p2, int period, int maxQueue)
        {
            stateMachine = new State2(p1, p2, maxQueue, period, new Random());
        }

        public Statistics Run(int N)
        {
            var st = new Statistics();
            var state = stateMachine.CurrentState();
            st.states.AddOrUpdate(state, 1, (key, oldValue) => oldValue + 1);

            for (int i = 0; i < N; i++)
            {
                stateMachine.tick();
                state = stateMachine.CurrentState();
                st.states.AddOrUpdate(state, 1, (key, oldValue) => oldValue + 1);
                st.Lq += stateMachine.Queue;
                st.Lc += stateMachine.InSystem;
                st.A += (stateMachine.Proc1Worked ? 1 : 0) + (stateMachine.Proc2Worked ? 1 : 0);

            }
            var p = stateMachine.NQ / (N / 2.0f);
            return st.Divide(N, stateMachine.wc, stateMachine.wq, p);
        }

        public struct Statistics
        {
            public ConcurrentDictionary<String, float> states;
            public float A;
            public float Q;
            public float Lc;
            public float Lq;
            public float Chan1;
            public float Chan2;
            public float Pbl;
            public float refuse;
            public float wc;
            public float wq;

            public Statistics()
            {
                states = new ConcurrentDictionary<string, float>();
                A = 0.0f;
                Lq = 0.0f;
                Lc = 0;
                Chan1 = 0.0f;
                Chan2 = 0.0f;
                Pbl = 0.0f;
                refuse = 0.0f;
                Q = 0;
                wc = 0;
                wq = 0;
            }

            public Statistics Divide(int N, float _wc, float _wq, float p)
            {
                List<string> keys = new List<string>(states.Keys);
                foreach (string key in keys)
                {
                    states[key] /= N;
                    if (key[2] == '1')
                    {
                        Chan1 += states[key];
                    }

                    if (key[3] == '1')
                    {
                        Chan2 += states[key];
                    }
                }
                wc = _wc;
                wq = _wq;

                A /= N;
                Lq /= N;
                Lc /= N;
                Pbl /= N;
                Q =p;
                refuse = 1 - Q;
                return this;
            }}}}
