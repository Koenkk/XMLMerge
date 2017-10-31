using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLMerge
{
    class Program
    {
        private static string datasetname = "AfasGetConnector";


        private static ArrayList initializeParameters(string directory, string filename)
        {
            ArrayList files = new ArrayList();

            // Verify that filename has at least 1 /
            string[] filenameSplit = filename.Split('\\');
            if (filenameSplit.Count() == 0 || filenameSplit.Last().Length == 0)
            {
                exitWithMessage("Invalid output path: " + filename);
            }

            // Verify that the directory exists
            string filenameDirectory = filename.Substring(0, filename.LastIndexOf("\\"));

            if(!Directory.Exists(filenameDirectory))
            {
                exitWithMessage("Directory: " + filenameDirectory + " does not exist");
            }

            // Verify that filename direcot
            if (File.Exists(filename))
            {
                exitWithMessage("File does already exist: " + filename);
            }

            // Check if directory exists
            if (Directory.Exists(directory))
            {
                // Check if any xml files exist in direcotry
                string[] fileList = Directory.GetFiles(directory);

                foreach (string file in fileList)
                {
                    if (file.EndsWith(".xml"))
                    {
                        files.Add(file);
                    }
                }
            }
            else
            {
                exitWithMessage("Directory: " + directory + " does not exist");
            }

            if (files.Count == 0)
            {
                exitWithMessage("No XML files found in directory: " + directory);
            }

            if (filename.Length == 0)
            {
                exitWithMessage("No XML files found in directory: " + directory);
            }


            return files;
        }

        private static void exitWithMessage(string message)
        {
            Console.WriteLine("ERROR: " + message);
            System.Environment.Exit(1);
        }

        private static void setTableNames(DataSet ds, string tableName)
        {
            foreach (DataTable d in ds.Tables)
            {
                d.TableName = tableName;
            }
        }
        
        private static DataSet mergeXMLFiles(ArrayList files, string tablename)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.DTD;

            // Create the first dataset and XMLreader
            DataSet ds = new DataSet();
            ds.DataSetName = datasetname;

            XmlReader xmlreader = XmlReader.Create((string) files[0], settings);

            try
            {
                ds.ReadXml(xmlreader);

                setTableNames(ds, tablename);
            } catch (Exception ex)
            {
                Console.WriteLine(files[0] + " contains invalid XML, skipped..");
            }

           

            for (int i = 1; i < files.Count; i++)
            {
                // Read out XML and merge with others
                string file = (string)files[i];
                XmlReader xmlreader2 = XmlReader.Create(file, settings);
                DataSet ds2 = new DataSet();
                ds2.DataSetName = datasetname;

                try
                {
                    ds2.ReadXml(xmlreader2);
                    setTableNames(ds2, tablename);
                    ds.Merge(ds2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(file + " contains invalid XML, skipped..");
                }
                
               
            }

            return ds;
        }

        private static string writeXML(DataSet ds, string directory, string output)
        { 
            ds.WriteXml(output, XmlWriteMode.WriteSchema);

            return output;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("XMLMerge v2.3");

            try
            {
                if (args.Length != 3)
                {
                    exitWithMessage("Incorrect amount of parameters, use <directory> <outputfile> <tablename>");
                }

                string directory = args[0];
                string outputFile = args[1];
                string tablename = args[2];

                if (outputFile.EndsWith("\\"))
                {
                    outputFile = outputFile + "output";
                }

                if (!outputFile.EndsWith(".xml"))
                {
                    outputFile = outputFile + ".xml";
                }

                // Intialize and check parameters
                ArrayList xmlFiles = initializeParameters(directory, outputFile);

                if (xmlFiles.Count == 1)
                {
                    exitWithMessage("Only 1 XML file found, no need to merge");
                }

                Console.WriteLine("Will merge the following XML files:");
                foreach (string s in xmlFiles)
                {
                    Console.WriteLine(s);
                }

                // Merge XML files 
                DataSet dataSet = mergeXMLFiles(xmlFiles, tablename);

                // Write merged XML file
                string output = writeXML(dataSet, directory, outputFile);
                Console.WriteLine("Merged XML to: " + output);
            } catch (Exception ex)
            {
                exitWithMessage("An error occured");
            }
        }
    }
}
