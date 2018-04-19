using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;

namespace termProject
{
    internal class Program
    {
        public static int jobs;
        public static int tabuCount;
        public static int candCount;
        public static Dictionary<int, process> processes;
        public static List<int> jobOrder;
        public static List<int> jobCosts;
        public static List<candidate> candList;
        public static candidate chosen;
        public static Random r;
        public static List<tabuEntry> tabuList;

        public class tabuEntry
        {
            public int id;
            public int swap1;
            public int swap2;
            public int tabuCount;

            public tabuEntry(int ident, int firtSwap, int secondSwap, int count)
            {
                id = ident;
                swap1 = firtSwap;
                swap2 = secondSwap;
                tabuCount = count;
            }

            public void subtractTabu()
            {
                tabuCount--;
            }

            public bool isTabu()
            {
                if (tabuCount == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        
        public class candidate
        {
            public int id;
            public List<int> jsOrder;
            public List<int> csOrder;
            public int objFunc;
            public int tabu;
            public int swap1;
            public int swap2;
            
            public candidate(int i)
            {
                id = i;
                jsOrder = new List<int>();
                csOrder = new List<int>();
                objFunc = 0;
                tabu = 0;
                swap1 = 0;
                swap2 = 0;
            }   
        }
        
        public class process
        {
            public int id { set; get; }
            public Dictionary<int, int> costs;
            
        }
        
        public static void Main(string[] args)
        {
            r = new Random();
            tabuCount = 5;
            jobs = 0;
            candCount = 0;
            processes = new Dictionary<int, process>();
            jobOrder = new List<int>();
            jobCosts = new List<int>();
            candList = new List<candidate>(); 
            chosen = new candidate(0);
            tabuList = new List<tabuEntry>();
            
            readInput("test.etp");
            printData();
            
            randAssign();
            //assignJobOrder();
            foreach (int a in jobOrder)
            {
                Console.Write(a + " ");
            }
            Console.WriteLine();
            
            listCosts();
            foreach (int b in jobCosts)
            {
                Console.Write(b + " ");
            }
            Console.WriteLine();
            calcObj();
            
            Console.WriteLine();
            tabu();
        }

        static public void tabu()
        {
                //generate initial candidate list
                populateCandidates(5);

                //Loop until converged or 100 iterations
            for (int i = 0; i < 100; i++)
            {
                
                //retrieve the current best candidate
                //lowest value z and not tabu
                //Add it to the tabu list
                chooseCandidate();

                //generate neighbors from the current candidate
                populateCandidates(5);
            }
        }

        static public void chooseCandidate()
        {
            foreach(candidate cc in candList.OrderBy(x => x.objFunc))
            {
                Console.WriteLine("Testing " + cc.objFunc);
                if (chosen.id == -1)
                {
                    chosen = cc;
                    //cc.tabu = tabuCount;
                }
            }
            
        }

        static public void adjustTabuCounts()
        {
            foreach(tabuEntry t in tabuList)
            {
                t.subtractTabu();
            }
        }

        static public void assignJobOrder()
        {
            jobOrder = new List<int>();
            jobCosts = new List<int>();
            jobs = 10;
            int[] js = {1, 8, 10, 9, 2, 3, 5, 4, 7, 6};
            int[] cs = {22, 24, 10, 24, 16, 13, 13, 16, 24};

            for (int i = 0; i < js.Length; i++)
            {
                jobOrder.Add(js[i]);
            }
            for (int i = 0; i < cs.Length; i++)
            {
                jobCosts.Add(cs[i]);
            }
        }        
        
        static public void populateCandidates(int n)
        {
            if (chosen.id > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    candCount++;
                    candidate x = new candidate(candCount);
                    randAssign();
                    listCosts();
                    x.jsOrder = jobOrder.ToList();
                    x.csOrder = jobCosts.ToList();
                    x.objFunc = calcObj();

                    candList.Add(x);
                }
            }
        }
        
        static public void printData()
        {
            foreach(KeyValuePair<int, process> kvp in processes)
            {
                Console.Write(kvp.Key + " :");
                foreach (KeyValuePair<int, int> x in kvp.Value.costs)
                {
                    Console.Write(x.Key + "," + x.Value + " ");
                }
                Console.Write("\n");
            }
        }
        
        static public void readInput(string filename)
        {
            string[] file = System.IO.File.ReadAllLines(filename);         

            string tmp = "";
            Dictionary<int, int> dynamicStorage = new Dictionary<int, int>();

            int procId = 0;
            for (int i = 0; i < file.Length; i++)
            {
                process newProc = new process();
                newProc.id = 0;
                newProc.costs = new Dictionary<int, int>();
                int idCount = 0;
                string[] line = file[i].Split(' ');
                for(int j=0; j<line.Length; j++)
                {
                    if (j == 0)
                    {
                        if (line[j].Equals("#"))
                        {
                            break;
                        }
                        else if (line[j].Equals("NumJobs"))
                        {
                            jobs = Convert.ToInt32(line[j+1]);
                            break;
                        }
                        else if (line[j].Equals(""))
                        {
                            break;
                        }
                        else
                        {
                            procId = procId + 1;
                            newProc.id = procId;
                            idCount++;
                            newProc.costs.Add(idCount, Convert.ToInt32(line[j]));
                        }
                    }
                    else
                    {
                        if (!line[j].Equals(""))
                        {
                            idCount++;
                            newProc.costs.Add(idCount, Convert.ToInt32(line[j]));
                        }
                    }                  
                }

                if (newProc.id > 0)
                {
                    processes.Add(newProc.id, newProc);
                }
            }         

        }

        static public void randAssign()
        {
            
            jobOrder = new List<int>();
            int count = jobs;
            List<int> jobList = new List<int>(processes.Keys);
            
            while (jobList.Count > 0)
            {
                int randJob = r.Next(0, jobList.Count);
                jobOrder.Add(jobList[randJob]);
                jobList.RemoveAt(randJob);
            }
        }

        static public void swapAssign()
        {
            jobOrder = new List<int>();
            int count = jobs;
            List<int> jobList = new List<int>(processes.Keys);
            
        }

        static public void listCosts()
        {
            jobCosts = new List<int>();
            for (int i = 0; i < jobOrder.Count-1; i++)
            {
                //Console.WriteLine(jobOrder[i] + " " + jobOrder[i+1]);
                jobCosts.Add(processes[jobOrder[i]].costs[jobOrder[i+1]]);
            }
        }
        
        static public int calcObj()
        {
            int deadline = jobs / 2;
            int counter = 1;
            int totalEarly = 0;
            int totalTardy = 0;
            for (int i = 0; i<deadline-1; i++)
            {
                totalEarly += counter * jobCosts[i];
                counter++;
            }

            //counter = 1;
            for (int i = deadline-1; i<= jobCosts.Count-1; i++)
            {
                totalTardy += counter * jobCosts[i];
                counter--;
            }

            int z = totalEarly + totalTardy;
            Console.WriteLine("The total Early = " + totalEarly);
            Console.WriteLine("The total Tarty = " + totalTardy);
            Console.WriteLine("The objective function result is : " + z);
            return z;
        }
    }
}