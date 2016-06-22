using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

/*
 *  synapse (A,B,weight) - has "weight" for each connection
 *  postsynapse (A,weight) - holds the accumulated weight per neuron
 *  read csv files into a List, connectome and postsynaptic
 *
 *  accept user input (neuron)
 *  runConnectome(neuron)
 *  dendriteAccumulate(neuron)
 *  fireNeuron(neuron)
 *
*/

namespace TheConnectome_CSharp
{
    internal class TheConnectomeDriver
    {
        // Global Lists
        private static readonly List<Synapse> Connectome = new List<Synapse>();

        private static readonly List<Synapse> Postsynaptic = new List<Synapse>();

        // CSV Files

        //  mac file strings
        //private static string connectomeFile = "/Users/vanessaulloa/RiderProjects/TheConnectome_CSharp/TheConnectome_CSharp/connectome.csv";
        //private static string postsynapticFile = "/Users/vanessaulloa/RiderProjects/TheConnectome_CSharp/TheConnectome_CSharp/postsynaptic.csv";
        //private static string connectomeFile = "/Users/vanessaulloa/RiderProjects/TheConnectome_CSharp/TheConnectome_CSharp/edgelist.csv";
        //private static string postsynapticFile = "/Users/vanessaulloa/RiderProjects/TheConnectome_CSharp/TheConnectome_CSharp/synaptic.csv";

        // windows file strings (VS Debug Folder)
        private static readonly string connectomeFile = "connectome.csv";

        private static readonly string postsynapticFile = "postsynaptic.csv";
        private static int muscleFireCount = 0;

        private static int neuronFireCount = 0;
        //private static string connectomeFile = "edgelist.csv";
        //private static string postsynapticFile = "synaptic.csv":

        // Global Variables
        private static int threshold = 15;

        private static Stopwatch connectomeRunTime = new Stopwatch();
        private static string userStimulatedNeuron;

        /*****  FUNCTIONS  *****/

        /*
         *  TestFilesWereRead()
         *  display contents of the Lists Connectome, Postsynaptic
         *  if data displays then the files were read correctly
         *
         */

        private static void TestFilesWereRead(StreamWriter writer)
        {
            writer.WriteLine("Displaying Contents of Connectome and Postsynaptic Lists:\n\n");

            int i;

            if (Connectome.Count > 0)
            {
                //Console.WriteLine("Connectome List size: " + Connectome.Count);
                writer.WriteLine("Connectome List size: " + Connectome.Count);

                for (i = 0; i < Connectome.Count; i++)
                {
                    //Console.WriteLine("\t" + i + " " + Connectome[i].NeuronA);
                    //Console.WriteLine("\t" + i + " " + Connectome[i].NeuronB);
                    //Console.WriteLine("\t" + i + " " + Connectome[i].Weight);

                    writer.WriteLine("\t" + i + " " + Connectome[i].NeuronA);
                    writer.WriteLine("\t" + i + " " + Connectome[i].NeuronB);
                    writer.WriteLine("\t" + i + " " + Connectome[i].Weight);
                }
            }
            else
            {
                Console.WriteLine("Connectome List has no data.");
                writer.WriteLine("Connectome List has no data.");
            }

            if (Postsynaptic.Count > 0)
            {
                Console.WriteLine("Postsynaptic List size: " + Postsynaptic.Count);

                for (i = 0; i < Postsynaptic.Count; i++)
                {
                    //Console.WriteLine("\t" + i + " " + Postsynaptic[i].NeuronA);
                    //Console.WriteLine("\t" + i + " " + Postsynaptic[i].Weight);

                    writer.WriteLine("\t" + i + " " + Postsynaptic[i].NeuronA);
                    writer.WriteLine("\t" + i + " " + Postsynaptic[i].Weight);
                }
            }
            else
            {
                //Console.WriteLine("Postsynaptic List has no data.");
                writer.WriteLine("Postsynaptic List has no data.");
            }

            writer.WriteLine("\n\n");
        }

        // end TestFilesWereRead()

        /*
         *  ReadConnectomeFile()
         *  read the .csv file being read into the Connectome List
         *  synapse(neuronA,neuronB,weight)
         *
         */

        private static void ReadConnectomeFile()
        {
            //int counter = 0;
            using (var reader = new StreamReader(File.OpenRead(connectomeFile)))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        // Console.WriteLine(line);

                        if (!string.IsNullOrEmpty(line))
                        {
                            var values = line.Split(',');

                            Connectome.Add(new Synapse(values[0], values[1], int.Parse(values[2])));
                            //Console.WriteLine("Added: [" + values[0] + "] [" + values[1] + "] [" + values[2] + "]");
                            //Console.WriteLine("Added " + Connectome[counter].NeuronA);
                            //counter++;
                        }
                        else
                        {
                            Console.WriteLine("line was null");
                        }
                    }
                    // end while
                }
                catch (Exception e)
                {
                    Console.WriteLine("File [" + connectomeFile + "] could not be read.");
                    Console.WriteLine("ERROR: " + e.Message);
                }
                // end try/catch

                reader.Close();
            }
            // end using
        }

        // end ReadConnectomeFile()

        /*
         *  ReadPostsynapticFile()
         *  read the .csv file being read into the Postsynaptic List
         *  Synapse(neuronA,weight)
         *
         */

        private static void ReadPostsynapticFile()
        {
            using (var reader = new StreamReader(File.OpenRead(postsynapticFile)))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        // Console.WriteLine(line);

                        if (!string.IsNullOrEmpty(line))
                        {
                            var values = line.Split(',');

                            Postsynaptic.Add(new Synapse(values[0], 0));
                        }
                        else
                        {
                            Console.WriteLine("line was null");
                        }
                    }
                    // end while
                }
                catch (Exception e)
                {
                    Console.WriteLine("File [" + postsynapticFile + "] could not be read.");
                    Console.WriteLine("ERROR: " + e.Message);
                }
                // end try/catch

                reader.Close();
            }
            // end using
        }

        // end ReadPostsynapticFile()

        /*
         *  DendriteAccumulate(Synapse)
         *
         *
         */

        private static void DendriteAccumulate(Synapse synapse, StreamWriter writer)
        {
            foreach (var con in Connectome)
            {
                if (con.NeuronA == synapse.NeuronA)
                {
                    foreach (var postsyn in Postsynaptic)
                    {
                        if (postsyn.NeuronA == con.NeuronB)
                        {
                            postsyn.Weight = con.Weight;

                            // output for file to show Postsynaptic Weight changes
                            Console.WriteLine("\tPostsynaptic Weight: {0} {1} + {2} =  {3}", postsyn.NeuronA, (postsyn.Weight - con.Weight), con.Weight, postsyn.Weight);
                            writer.WriteLine("\tPostsynaptic Weight: {0} {1} + {2} =  {3}", postsyn.NeuronA, (postsyn.Weight - con.Weight), con.Weight, postsyn.Weight);
                        }
                    }
                }
            }
        }

        // end DendriteAccumulate(Synapse)

        /*
         *  FireNeuron(Synapse)
         *
         *
         */

        private static void FireNeuron(Synapse neuron, StreamWriter writer)
        {
            DendriteAccumulate(neuron, writer);
            foreach (var postsyn in Postsynaptic)
            {
                if (Math.Abs(postsyn.Weight) > threshold)
                {
                    if (postsyn.NeuronA == "PLMR" || postsyn.NeuronA == "PLML" ||
                        postsyn.NeuronA.Substring(0, 2) == "MV" ||
                        postsyn.NeuronA.Substring(0, 2) == "MD")
                    {
                        muscleFireCount++;
                        Console.WriteLine("Fire Muscle {0}{1}", postsyn.NeuronA, postsyn.NeuronB);
                        writer.WriteLine("Fire Muscle {0}{1}", postsyn.NeuronA, postsyn.NeuronB);
                        postsyn.ResetWeight = 0;
                    }
                    else
                    {
                        neuronFireCount++;
                        Console.WriteLine("Fire Neuron {0}", postsyn.NeuronA);
                        writer.WriteLine("Fire Neuron {0}", postsyn.NeuronA);
                        DendriteAccumulate(postsyn, writer);
                        postsyn.ResetWeight = 0;
                    }
                }
            }
        }

        // end FireNeuron(Synapse)

        /*
         *  RunConnectome(Synapse)
         *
         *
         */

        private static void RunConnectome(Synapse neuron, StreamWriter writer)
        {
            DendriteAccumulate(neuron, writer);

            foreach (var postsyn in Postsynaptic)
            {
                if (Math.Abs(postsyn.Weight) > threshold)
                {
                    FireNeuron(postsyn, writer);
                    postsyn.ResetWeight = 0;
                }
            }
        }

        // end RunConnectome(Synapse)

        private static void Main(string[] args)
        {
            /*****  BEGIN  *****/
            // user input (Neuron) create output file, includes neuron and run start time read csv files

            Console.Write("Please enter a Neuron: ");
            userStimulatedNeuron = Console.ReadLine().Trim().ToUpper();

            // start the Stopwatch object to gather run time
            connectomeRunTime.Start();

            /*****  BEGIN OUTPUT FILE CREATION  *****/

            string programRunStartDateTimeFileName = string.Format("\\OutputFiles\\{0}_{1:dd-MM-yyyy_hh-mm-ss}.dat", userStimulatedNeuron,
                DateTime.Now);
            string programRunStartDateTime = string.Format("{0:dd-MMM-yyyy, hh-mm-ss tt}", DateTime.Now);

            string path = string.Concat(Environment.CurrentDirectory, @programRunStartDateTimeFileName);
            StreamWriter writer = new StreamWriter(path);

            /***** BEGIN FILE READ  *****/

            ReadConnectomeFile();
            ReadPostsynapticFile();
            //TestFilesWereRead(writer);

            if (String.IsNullOrEmpty(userStimulatedNeuron))
            {
                Console.WriteLine("You entered invalid data. Please try again.");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Neuron Stimulated: {0}", userStimulatedNeuron);
                Console.WriteLine("\nThe Connectome Program Start!");
                Console.WriteLine("Rune Date and Time: {0}", programRunStartDateTime);
                Console.WriteLine("\n");

                writer.WriteLine("\nThe Connectome Program Start!");
                writer.WriteLine("Rune Date and Time: {0}", programRunStartDateTime);
                writer.WriteLine("\n");
                writer.WriteLine("Neuron Stimulated: {0}", userStimulatedNeuron);
            }

            /*****  END  *****/

            /*****  BEGIN THREADS  *****/
            // create threads for multi-procesing

            /*****  END THREADS  *****/

            /*****  START RUNCONNECTOME  *****/
            // pass all instances of inputted neuron to runConnectome()

            foreach (var synapse in Connectome)
            {
                if (synapse.NeuronA == userStimulatedNeuron)
                {
                    Console.WriteLine("\n**********");
                    Console.WriteLine("Synapse Stimulated: {0}, {1}", synapse.NeuronA, synapse.NeuronB);
                    Console.WriteLine("**********\n");

                    writer.WriteLine("\n**********");
                    writer.WriteLine("Synapse Stimulated: {0}, {1}", synapse.NeuronA, synapse.NeuronB);
                    writer.WriteLine("**********\n");

                    RunConnectome(synapse, writer);
                }
            }

            // program has finished RunConnectome
            connectomeRunTime.Stop();

            /*****  END RUNCONNECTOME  *****/

            /*****  END OF PROGRAM DATA  *****/
            // collected outputs for information count of how many neurons fired count of how many
            // muscles fired total run time of program

            Console.WriteLine("\n\n**********");
            Console.WriteLine("Total Neurons Fired: {0}", neuronFireCount);
            Console.WriteLine("Total Musicles Fired: {0}", muscleFireCount);
            Console.WriteLine("Total Run Time: {0:hh\\:mm\\:ss} \n", connectomeRunTime.Elapsed);
            Console.WriteLine("**********\n");

            writer.WriteLine("\n\n**********");
            writer.WriteLine("\n\nTotal Neurons Fired: {0}", neuronFireCount);
            writer.WriteLine("Total Musicles Fired: {0}", muscleFireCount);
            writer.WriteLine("Total Run Time: {0:hh\\:mm\\:ss} \n", connectomeRunTime.Elapsed);
            writer.WriteLine("**********\n");

            /*****  END OF PROGRAM DATA END  *****/

            writer.Close();
            Console.WriteLine("\nPress any key to exit...");
            Console.Read(); //  keeps console window open
        }

        // end main
    }

    // end class
}

// end namespace