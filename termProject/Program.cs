using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
        public static candidate best;
        public static Random r;
        public static List<tabuEntry> tabuList;
        public static int iterNum;
        public static int maxIters;
        public static int bestObj;

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

            public candidate copyTo(candidate temp)
            {
                temp.id = id;
                temp.jsOrder = jsOrder.ToList();
                temp.csOrder = csOrder.ToList();
                temp.objFunc = objFunc;
                temp.tabu = tabu;
                temp.swap1 = swap1;
                temp.swap2 = swap2;

                return temp;
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
            maxIters = 10;
            processes = new Dictionary<int, process>();
            jobOrder = new List<int>();
            jobCosts = new List<int>();
            candList = new List<candidate>(); 
            chosen = new candidate(0);
            best = new candidate(0);
            tabuList = new List<tabuEntry>();
            iterNum = 0;
            bestObj = 0;
            
            readInput("test.etp");
            printData();
            
            randAssign();
            assignJobOrder();
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
            
            Console.WriteLine();
            Console.WriteLine("##### Best OBJ : " + bestObj + " ####");
            foreach (int a in best.jsOrder)
            {
                Console.Write(a + " ");
            }
            Console.WriteLine("End of jsOrder");

            //listCosts();
            foreach (int b in best.csOrder)
            {
                Console.Write(b + " ");
            }
            Console.WriteLine("End of csOrder");
            Console.WriteLine("Best Obj = " + best.objFunc);
        }

        static public void tabu()
        {            
                //generate initial candidate list
                populateCandidates(tabuCount);

                //Loop until converged or 100 iterations
            for (int i = 0; i < maxIters; i++)
            {
                iterNum = i+1;
                Console.WriteLine("Iteration number " + i);
                //retrieve the current best candidate
                //lowest value z and not tabu
                //Add it to the tabu list
                chooseCandidate();
                
                //Calculate new Objective Function for the chosen one
                calcObj();

                //generate neighbors from the current candidate
                populateCandidates(tabuCount);
                
                adjustTabuCounts();
            }
        }

        static public void chooseCandidate()
        {
            //shortcut?
            chosen = new candidate(0);
            chosen = candList.OrderBy(x => x.objFunc).First();
            //Console.WriteLine("Testing " + chosen.id + " :" + chosen.objFunc + " listLength = " + candList.Count + " " + candList.OrderByDescending(x => x.objFunc).First().objFunc);
            
            /*
            foreach(candidate cc in candList.OrderBy(x => x.objFunc))
            {
                Console.WriteLine("Testing " + cc.id + " :" + cc.objFunc + " listLenght = " + candList.Count);
                if (chosen.id == -1)
                {
                    chosen = cc;
                    //cc.tabu = tabuCount;
                }
            }
            */
            
            //reduce the candidate list to maintain size
            if (candList.Count > tabuCount)
            {
                int extra = candList.OrderByDescending(x => x.objFunc).First().id;
                candList.RemoveAt(candList.FindIndex(x => x.id == extra));
            }                     
        }

        static public void adjustTabuCounts()
        {
            for(int i=0; i<tabuList.Count; i++)
            {
                tabuList[i].subtractTabu();
                if (tabuList[i].tabuCount  == 0)
                {
                    //int loc = tabuList.IndexOf(t);
                    tabuList.RemoveAt(i);
                }
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
                //perform the swap assign
                swapAssign();
                listCosts();
                candList.Add(chosen);
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    candCount++;
                    chosen = new candidate(candCount);
                    randAssign();
                    listCosts();
                    chosen.jsOrder = jobOrder.ToList();
                    chosen.csOrder = jobCosts.ToList();
                    chosen.objFunc = calcObj();

                    candList.Add(chosen);
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
            candidate temp = chosen;

            int s1 = 0;
            int s2 = 0;
            if (tabuList.Count == 0)
            {
                s1 = chosen.jsOrder[r.Next(0, (jobs - 1))];
                s2 = s1;
                while (s2 != s1)
                {
                    s2 = chosen.jsOrder[r.Next(0, (jobs - 1))];
                }
            }
            else
            {                
                bool repeat = true;
                while (repeat)
                {
                    s1 = chosen.jsOrder[r.Next(0, (jobs - 1))];
                    s2 = s1;
                    while (s2 == s1)
                    {
                        s2 = chosen.jsOrder[r.Next(0, (jobs - 1))];
                    }
                    
                    //make sure that the values do no exist in the tabu list
                    foreach (tabuEntry te in tabuList)
                    {
                        if ((s1 == te.swap1 && s2 == te.swap2) || (s1 == te.swap2 && s2 == te.swap1))
                        {
                            break;
                        }

                        repeat = false;
                    }
                }
            }
            tabuEntry t = new tabuEntry(iterNum, s1, s2, tabuCount);
            tabuList.Add(t);

            theSwap(s1, s2);
        }

        static public void theSwap(int index1, int index2)
        {
            int i1 = -1;
            int i2 = -1;
            for(int i=0; i<chosen.jsOrder.Count; i++)
            {
                if (chosen.jsOrder[i] == index1)
                {
                    i1 = i;
                }
                if (chosen.jsOrder[i] == index2)
                {
                    i2 = i;
                }

                if (i1>=0 && i2>=0)
                    break;
            }
            
            if(i1 < 0 || i2 < 0)
                Console.WriteLine("Could not find our swap values " + index1 + " " + index2);

            Console.WriteLine("Swapping " + index1 + " and " + index2);
            chosen.jsOrder[i1] = index2;            
            chosen.jsOrder[i2] = index1;
            chosen.id += iterNum;
            candList.Add(chosen);

        }
        
        static public void listCosts()
        {            
            if (chosen.id > 0)
            {
                chosen.csOrder = new List<int>();
                
                for (int i = 0; i < chosen.jsOrder.Count - 1; i++)
                {
                    chosen.csOrder.Add(processes[chosen.jsOrder[i]].costs[chosen.jsOrder[i + 1]]);                    
                }
            }
            else
            {                
                jobCosts = new List<int>();
                for (int i = 0; i < jobOrder.Count - 1; i++)
                {
                    jobCosts.Add(processes[jobOrder[i]].costs[jobOrder[i + 1]]);
                }
            }
        }
        
        static public int calcObj()
        {
            int deadline = jobs / 2;
            int counter = 1;
            int totalEarly = 0;
            int totalTardy = 0;

            Console.WriteLine("BLAH BLAH BLAH " + chosen.id);
            if (chosen.id == 0)
            {
                for (int i = 0; i < deadline - 1; i++)
                {
                    totalEarly += counter * jobCosts[i];
                    counter++;
                }

                //counter = 1;
                for (int i = deadline - 1; i <= jobCosts.Count - 1; i++)
                {
                    totalTardy += counter * jobCosts[i];
                    counter--;
                }
            }
            else
            {
                for (int i = 0; i < deadline - 1; i++)
                {
                    totalEarly += counter * chosen.jsOrder[i];
                    counter++;
                }

                //counter = 1;
                for (int i = deadline - 1; i <= chosen.csOrder.Count - 1; i++)
                {
                    totalTardy += counter * chosen.csOrder[i];
                    counter--;
                }
            }

            if (chosen.id > 0)
            {
                foreach (int a in chosen.jsOrder)
                {
                    Console.Write(a + " ");
                }
                Console.WriteLine("End of jsOrder");

                //listCosts();
                foreach (int b in chosen.csOrder)
                {
                    Console.Write(b + " ");
                }
                Console.WriteLine("End of csOrder");
            }
            
            int z = totalEarly + totalTardy;
            Console.WriteLine("The process list : " + chosen.jsOrder.Count + " " + deadline);
            Console.WriteLine("The total Early = " + totalEarly);
            Console.WriteLine("The total Tarty = " + totalTardy);
            Console.WriteLine("The objective function result is : " + z);
            if (bestObj == 0)
            {
                bestObj = z;
                best = chosen.copyTo(best);
            }
            else
            {
                if (z < bestObj)
                {
                    bestObj = z;
                    best = chosen.copyTo(best);
                }
            }
            
            return z;
        }
    }
}