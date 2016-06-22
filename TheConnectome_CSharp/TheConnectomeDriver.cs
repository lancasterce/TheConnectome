using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
        private static void Main(string[] args)
        {
            // VARIABLES
            Stopwatch connectomeRunTime = new Stopwatch();
            string userStimulatedNeuron = "";

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

            TheConnectome.ReadConnectomeFile();
            TheConnectome.ReadPostsynapticFile();
            //TestFilesWereRead(writer);

            if (String.IsNullOrEmpty(userStimulatedNeuron))
            {
                // close the StreamWriter object in order to access the file move the file to
                // appropriate folder to isolate from successful runs delete original file

                writer.Close();
                Console.WriteLine("You entered invalid data. Please try again.");
                Console.Read();

                // moved failed output file to folder
                string failedProgramRunStartDateTimeFileName = string.Format("\\OutputFiles\\Failed\\{0}_{1:dd-MM-yyyy_hh-mm-ss}.dat", userStimulatedNeuron,
                DateTime.Now);
                string failedPath = string.Concat(Environment.CurrentDirectory, @failedProgramRunStartDateTimeFileName);
                File.Move(path, failedPath);
                File.Delete(path);

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

            foreach (var synapse in TheConnectome.Connectome)
            {
                if (synapse.NeuronA == userStimulatedNeuron)
                {
                    Console.WriteLine("\n**********");
                    Console.WriteLine("Synapse Stimulated: {0}, {1}", synapse.NeuronA, synapse.NeuronB);
                    Console.WriteLine("**********\n");

                    writer.WriteLine("\n**********");
                    writer.WriteLine("Synapse Stimulated: {0}, {1}", synapse.NeuronA, synapse.NeuronB);
                    writer.WriteLine("**********\n");

                    TheConnectome.RunConnectome(synapse, writer);
                }
            }

            // program has finished RunConnectome
            connectomeRunTime.Stop();

            /*****  END RUNCONNECTOME  *****/

            /*****  END OF PROGRAM DATA  *****/
            // collected outputs for information count of how many neurons fired count of how many
            // muscles fired total run time of program

            Console.WriteLine("\n\n**********");
            Console.WriteLine("Total Neurons Fired: {0}", TheConnectome.NeuronFireCount);
            Console.WriteLine("Total Musicles Fired: {0}", TheConnectome.MuscleFireCount);
            Console.WriteLine("Total Run Time: {0:hh\\:mm\\:ss} \n", connectomeRunTime.Elapsed);
            Console.WriteLine("**********\n");

            writer.WriteLine("\n\n**********");
            writer.WriteLine("\n\nTotal Neurons Fired: {0}", TheConnectome.NeuronFireCount);
            writer.WriteLine("Total Musicles Fired: {0}", TheConnectome.MuscleFireCount);
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