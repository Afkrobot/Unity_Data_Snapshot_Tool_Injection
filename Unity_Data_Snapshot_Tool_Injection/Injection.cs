using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Steamworks;
using System.IO.IsolatedStorage;

namespace Unity_Data_Snapshot_Tool_Injection
{
    class Injection : MonoBehaviour
    {
        /// <summary>
        /// Folder path for snapshot save.
        /// </summary>
        private static string path;

        /// <summary>
        /// List of exceptions encountered during snapshot creation
        /// </summary>
        private List<Exception> eList = new List<Exception>();

        public void Start()
        {
        }

        public void Update()
        {
            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }
        }

        public void OnGUI()
        {
            //Generates a Unity.GUI button in the top left corner, if statement executes once on button release.
            if (GUI.Button(new Rect(0, 0, 125f, 50f), "Save Snapshot"))
            {
                path = Application.dataPath + "/snapshots/" +
                    DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");

                //Grabs all currently loaded gameobjects in the scene. May contain some that are not visible but still loaded.
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                try
                {
                    //We iterate over all the transform components contained within our game object (gObject) as every game object in unity has one
                    //therefore this is the only reliable way of grabbing each and every child of a game object.
                    foreach (GameObject gObject in allObjects)
                    {
                        RecursiveGetObjects(gObject.GetComponent<Transform>());
                    }
                }
                catch (Exception e)
                {
                    eList.Add(e);
                }

                //The Exceptions file is created and written to. This is necessary for debugging as Unity will just eat exceptions.
                //Note that exceptions thrown by injections are less helpful than usual as the program will often be unsure where exactly they came from.
                string filePath = path + "/0-Exceptions.txt";
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    for (int i = 0; i < eList.Count; i++)
                    {
                        sw.WriteLine(i + ": " + eList[i].ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Grabs all of a parent Transforms children and performs our operations on them, which in this case is creating files containing
        /// each game objects data.
        /// </summary>
        /// <param name="a"></param>
        private void RecursiveGetObjects(Transform a)
        {
            //We get the gameo object attached to the transform
            GameObject gObject = a.gameObject;
            if (gObject != null)
            {
                string filePath = FormatFilename(gObject.name);
                //Grabs all of the components on a game object.
                Component[] components = gObject.GetComponents(typeof(Component));
                if (components.Length > 0)
                {
                    if (!File.Exists(filePath))
                    {
                        File.Create(filePath).Close();
                    }
                    try
                    {
                        //Writes each components data to file
                        using (StreamWriter sw = new StreamWriter(filePath))
                        {
                            //First we log the data on the game object
                            {
                                sw.WriteLine(gObject.ToString());
                                PropertyInfo[] properties = gObject.GetType().GetProperties();
                                foreach(PropertyInfo property in properties)
                                {
                                    sw.WriteLine(PropertyDataString(property, gObject));
                                }
                            }
                            sw.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

                            //Then on each of its components.
                            foreach (Component component in components)
                            {
                                sw.WriteLine(component.ToString());
                                PropertyInfo[] properties = component.GetType().GetProperties();
                                foreach(PropertyInfo property in properties)
                                {
                                    sw.WriteLine(PropertyDataString(property, component));
                                }
                                sw.WriteLine("##################################################");
                            }

                            sw.Close();
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        //I know, we have all learned that if you catch an exception, it is good form to actually handle it, but in this case its just a lot of exceptions telling me
                        //that some of the fields im checking are not set. I know they are not and it does not matter for the programs functionality so I just leave these exceptions be.
                    }
                    catch (Exception e)
                    {
                        eList.Add(e);
                    }
                }
                //If there is still children left we grab them from the parent transform
                //and perform the same operation to look if they too have children
                if (a.childCount > 0)
                {
                    foreach (Transform b in a)
                    {
                        RecursiveGetObjects(b);
                    }
                }
            }
        }

        /// <summary>
        /// Takes an object and the info of the property we want to check for that object.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="obj"></param>
        /// <returns>A formated string containing info on and the data of the property we checked.</returns>
        private string PropertyDataString(PropertyInfo property, object obj)
        {
            string pName = ((MemberInfo)property).Name;
            object pValue = property.GetValue(obj, null);

            //We check to see if it is an array because if it is, we format it differently for readability.
            if (!property.PropertyType.IsArray)
            {
                //We change the formating depending on what the objects type is, to make it more informative/neat.
                if (property.PropertyType == typeof(Mesh) || property.PropertyType == typeof(Sprite) || property.PropertyType == typeof(GameObject))
                {
                    return "(" + pValue.GetType().ToString() + ") " + pName + " = " + ((UnityEngine.Object)pValue).name;
                }
                else if (property.PropertyType == typeof(Matrix4x4))
                {
                    string mat = "\n" + pValue.ToString();
                    mat = mat.Substring(0, mat.Length - 1);
                    return "(" + pValue.GetType().ToString() + ") " + pName + " = " + mat;
                }
                else
                {
                    return "(" + pValue.GetType().ToString() + ") " + pName + " = " + pValue.ToString();
                }
            }
            else
            {
                //Initiates the string as [] for if there are no array items.
                string values = "[]";
                Array propertyArray = (Array)pValue;
                if (propertyArray.Length > 0)
                {
                    //Builds an indented string, neatly listing each array item.
                    values = "\n";
                    foreach (var pArrayItem in propertyArray)
                    {
                        values += new string(' ', pArrayItem.GetType().ToString().Length) + "- " + ((UnityEngine.Object)pArrayItem).name + "\n";
                    }
                    values = values.Substring(0, values.Length - 1);
                }
                return "(" + pValue.ToString() + ") " + pName + " = " + values;
            }
        }

        /// <summary>
        /// Turns given file names into valid file paths for saving.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>A string representing a valid file path for the given file name. File name might be truncated.</returns>
        private string FormatFilename (string fileName)
        {
            //Replaces most invalid characters
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            //Strips all non ASCII characters (for example: Japanese Characters)
            fileName = Encoding.ASCII.GetString(Encoding.Convert(
                Encoding.UTF8,
                Encoding.GetEncoding(
                    Encoding.ASCII.EncodingName,
                    new EncoderReplacementFallback(string.Empty),
                    new DecoderExceptionFallback()
                    ),
                Encoding.UTF8.GetBytes(fileName)
            ));
            //Truncates file name so that the resulting file path is a windows legal length. (MAX is 260)
            if (path.Length + fileName.Length > 259)
            {
                //The "-5" is because we still need to add a "/" and the ".txt" to the trunctuated file name. Thats 5 more characters.
                int maxLength = 259 - path.Length - 5;
                fileName = fileName.Substring(0, maxLength);
            }
            return path + "/" + fileName + ".txt";
        }
    }
}

