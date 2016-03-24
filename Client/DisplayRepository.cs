///////////////////////////////////////////////////////////////////////
// DisplayRepository.cs - contains methods to display information    //
//                      stored in Repository .                       //
//                                                                   //                                                                  
// ver 1.0                                                           //
// Language:    C#, .Net Framework 4.5                               //
// Platform:    Sony Vaio, Win8.1, SP1                               //
// Application: Demonstration for CSE681, Project #2, Fall 2014      //
// Author:      Gaurav Bhasin, Syracuse University                   //
//              (315) 744-5233, gabhasin@syr.edu                     //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package display the Repository information to standard output and also has method to print the information to a XML file
 * based on the options provided
 * 
 * Public Interface:
 * ------------------
 *      public static void displayTypeAnalysis(Repository<Elem>[] repArray, bool typefunctionanalysis); //prints type analysis
 *      public static void displayRelationAnalysis(Repository<ElemForRel> rep, bool relationAnalysis); //prints relatuonships
 *      public static void xmlOutput(Repository<Elem>[] repoArray, Repository<ElemForRel> repoRel, bool recurse, bool relAnalysis)// for saving output to XML file
 *  
 */
/* Required Files:
 *   -Repository.cs
 *   
 *
 *   
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryNS;
using System.Xml;
using System.Xml.Serialization;

namespace DisplayPkg
{
    public class DisplayRepository
    {
        //method for displaying repository for each file holing types and their function defined + number of scopes in the functions
        public static void displayTypeAnalysis(Repository<Elem>[] repArray, bool typefunctionanalysis)
        {
            if (typefunctionanalysis)
            {
                Console.WriteLine("*************************************************");
                Console.WriteLine("             Type Analysis                       ");
                Console.WriteLine("*************************************************");
                Console.WriteLine("\n\n\n");

                foreach (Repository<Elem> rep in repArray)
                {
                    Console.WriteLine("\n\n\n");
                    Console.WriteLine("Analysing File :  " + rep.AttachedFile);
                    Console.WriteLine("\n  {0,-15}{1,-30}{2,-15}{3,-15}{4,-15}", "TYPE", "TYPENAME", "BEGIN AT", "END AT", "SCOPECOUNT");
                    Console.WriteLine("***********************************************************************************************************");
                    Console.WriteLine("\n");
                    if (rep.Locations.Count != 0)
                        foreach (Elem e in rep.Locations)
                        {
                            Console.WriteLine(e.ToString());

                        }
                    else Console.WriteLine("                   Nothing to Analyse In this file\n\n\n");
                }
            }

        }
        //method for dispalying detected relationships between the types
        public static void displayRelationAnalysis(Repository<ElemForRel> rep, bool relationAnalysis)
        {
            if (relationAnalysis)
            {
                Console.WriteLine("*************************************************");
                Console.WriteLine("             Relation Analysis                   ");
                Console.WriteLine("*************************************************");

                Console.WriteLine("\n\n\n");
                //Aggregation Records
                Console.WriteLine("Classes sharing \"Aggreation\" relaionship");
                Console.WriteLine("***********************************************************************************************************\n\n");

                Console.WriteLine("\n  {0,-30}|{1,-15}|{2,-30}|{3,-25}|{4,-15}|{5,-30}|{6,-20}|{7,-5}\n", "FileName", "Relationship", "TYPE", "NAME", "        ", "TYPE", "TYPENAME", "At Line");
                foreach (ElemForRel e in rep.Locations)
                {
                    if (e.Relation == "Aggregation")
                    {
                       
                        Console.WriteLine(e.ToString());
                    }
                }
                Console.WriteLine("\n\n\n");

                //Inhertience Records
                Console.WriteLine("Classes sharing \"Inheritence\" relaionship");
                Console.WriteLine("***********************************************************************************************************\n\n");
                Console.WriteLine("\n  {0,-30}|{1,-15}|{2,-30}|{3,-25}|{4,-15}|{5,-30}|{6,-20}|{7,-5}\n", "FileName", "Relationship", "TYPE", "NAME", "        ", "TYPE", "TYPENAME", "At Line");
                foreach (ElemForRel e in rep.Locations)
                {
                    if (e.Relation == "Inheritence")
                    {
                        
                        Console.WriteLine(e.ToString());
                    }
                }
                Console.WriteLine("\n\n\n");
                //Composition Records
                Console.WriteLine("Classes sharing \"Composition\" relaionship");
                Console.WriteLine("***********************************************************************************************************\n\n");
                Console.WriteLine("\n  {0,-30}|{1,-15}|{2,-30}|{3,-25}|{4,-15}|{5,-30}|{6,-20}|{7,-5}\n", "FileName", "Relationship", "TYPE", "NAME", "        ", "TYPE", "TYPENAME", "At Line");
                foreach (ElemForRel e in rep.Locations)
                {
                    
                    if (e.Relation == "Composition")
                    {
                        
                        Console.WriteLine(e.ToString());
                    }
                }
                Console.WriteLine("\n\n\n");
                //Using Records
                Console.WriteLine("Classes sharing \"Using\" relaionship");
                Console.WriteLine("***********************************************************************************************************\n\n");

                Console.WriteLine("\n  {0,-30}|{1,-15}|{2,-30}|{3,-25}|{4,-15}|{5,-30}|{6,-20}|{7,-5}\n", "FileName", "Relationship", "TYPE", "NAME", "        ", "TYPE", "TYPENAME", "At Line");
                foreach (ElemForRel e in rep.Locations)
                {
                    if (e.Relation == "Using")
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
        //method to save the information in XML file
        public static void xmlOutput(Repository<Elem>[] repoArray, Repository<ElemForRel> repoRel, bool recurse, bool relAnalysis)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("ManagedCodeAnalizer");
                xmlDoc.AppendChild(rootNode);
                XmlNode typeAnalysis = xmlDoc.CreateElement("typeLocations");
                rootNode.AppendChild(typeAnalysis);

                foreach (Repository<Elem> rep in repoArray)
                {
                    int index = rep.AttachedFile.LastIndexOf("\\");
                    string fName = rep.AttachedFile.Substring(index + 1, rep.AttachedFile.Length - 1 - index);
                    XmlNode File = xmlDoc.CreateElement(fName);
                    typeAnalysis.AppendChild(File);
                    foreach (Elem e in rep.Locations)
                    {
                        XmlNode Type = xmlDoc.CreateElement(e.name);
                        XmlAttribute type = xmlDoc.CreateAttribute("type");
                        type.Value = e.type;
                        XmlAttribute name = xmlDoc.CreateAttribute("name");
                        name.Value = e.type;
                        XmlAttribute begin = xmlDoc.CreateAttribute("begin");
                        begin.Value = e.begin.ToString();
                        XmlAttribute end = xmlDoc.CreateAttribute("end");
                        end.Value = e.end.ToString();
                        XmlAttribute complexity = xmlDoc.CreateAttribute("complexity");
                        complexity.Value = e.funcComplexity.ToString();

                        Type.Attributes.Append(type);
                        Type.Attributes.Append(begin);
                        Type.Attributes.Append(end);
                        Type.Attributes.Append(complexity);
                        File.AppendChild(Type);
                        typeAnalysis.AppendChild(File);
                    }

                }

                if (relAnalysis)
                {
                    XmlNode RelationAnalysis = xmlDoc.CreateElement("RelAnalysis");
                    rootNode.AppendChild(RelationAnalysis);
                    foreach (ElemForRel e in repoRel.Locations)
                    {
                        XmlNode Relations = xmlDoc.CreateElement(e.Relation);
                        XmlAttribute fileName = xmlDoc.CreateAttribute("fileName");
                        fileName.Value = e.fileName;
                        XmlAttribute type1 = xmlDoc.CreateAttribute("type1");
                        type1.Value = e.type1;
                        XmlAttribute type1Name = xmlDoc.CreateAttribute("type1Name");
                        type1Name.Value = e.type1Name;
                        XmlAttribute type2 = xmlDoc.CreateAttribute("type2");
                        type2.Value = e.type2;
                        XmlAttribute type2Name = xmlDoc.CreateAttribute("type2Name");
                        type2.Value = e.type2Name;
                        XmlAttribute lineNo = xmlDoc.CreateAttribute("lineNo");
                        lineNo.Value = e.lineNumber.ToString();

                        Relations.Attributes.Append(fileName);
                        Relations.Attributes.Append(type1);
                        Relations.Attributes.Append(type1Name);
                        Relations.Attributes.Append(type2);
                        Relations.Attributes.Append(type2Name);
                        Relations.Attributes.Append(lineNo);
                        RelationAnalysis.AppendChild(Relations);
                    
                    }

                    String path = System.IO.Directory.GetCurrentDirectory();

                    xmlDoc.Save("ManagedCodeAnalysis.xml");
                    Console.WriteLine("{0}\\ManagedCodeAnalysis.xml", path);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in XML Handling {0,5}", e.Message);
            }

        }
    }
}
    
    
