//-----------------------------------------------------------------------
// <copyright file="IniSerializer.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class IniSerializer
    {
        public static List<object> DeserializeIni(string iniString)
        {
            var objects = new Dictionary<string, object>();
            Type currentType = null;
            object currentObject = null;

            foreach (var line in GetLines(iniString))
            {
                // Detecting if a class type has been specified
                if (IsTypeLine(line, out string typeName))
                {
                    if (objects.TryGetValue(typeName, out currentObject) == false)
                    {
                        currentType = Type.GetType(typeName);

                        if (currentType == null)
                        {
                            Console.WriteLine($"ERROR - Line {line.Nunber}: Unknown Type {typeName}");
                        }
                        else
                        {
                            currentObject = Activator.CreateInstance(currentType);
                            objects.Add(typeName, currentObject);
                        }
                    }

                    continue;
                }

                // Skipping lines that don't meet the correct formatting
                if (IsValidPropertyLine(line, out int equalsIndex) == false)
                {
                    continue;
                }

                string propertyName = line.Value.Substring(0, equalsIndex).Trim();
                string propertyStringValue = line.Value.Substring(equalsIndex + 1).Trim();
                bool isListAdd = propertyName.StartsWith("+");
                bool isListRemove = propertyName.StartsWith("-");

                if (isListAdd || isListRemove)
                {
                    propertyName = propertyName.Substring(1);
                }

                if (currentType == null || currentObject == null)
                {
                    Console.WriteLine($"ERROR - Line {line.Nunber}: Trying to set property {propertyName} on unknown object!");
                    continue;
                }

                var propertyInfo = currentType.GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    Console.WriteLine($"ERROR - Line {line.Nunber}: Trying to set property {propertyName} that doesn't exist in type {currentType.FullName}!");
                    continue;
                }

                bool isGenericListProperty = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                bool startsWithListCharacter = isListAdd || isListRemove;

                if (isGenericListProperty && startsWithListCharacter == false)
                {
                    Console.WriteLine($"ERROR - Line {line.Nunber}: Encountered List<> property {propertyName} that does not start with a '+' or '-'!");
                    continue;
                }
                else if (isGenericListProperty == false && startsWithListCharacter)
                {
                    Console.WriteLine($"ERROR - Line {line.Nunber}: Encountered property {propertyName} that is not a List<>, but starts with a '+' or '-'!");
                    continue;
                }

                // Now setting the property or add/removing an item from a list
                if (isGenericListProperty)
                {
                    SetGenericListProperty(currentObject, line.Nunber, propertyInfo, propertyStringValue, isListAdd);
                }
                else
                {
                    SetProperty(currentObject, line.Nunber, propertyInfo, propertyStringValue);
                }

                static void SetGenericListProperty(object instance, int lineCount, PropertyInfo property, string propertyStringValue, bool isAdd)
                {
                    var genericListType = property.PropertyType.GenericTypeArguments[0];

                    if (TryParse(lineCount, property.Name, genericListType, propertyStringValue, out object result))
                    {
                        var list = property.GetValue(instance);

                        if (list == null)
                        {
                            property.SetValue(instance, Activator.CreateInstance(property.PropertyType));
                            list = property.GetValue(instance);
                        }

                        var method = isAdd ? list.GetType().GetMethod("Add") : list.GetType().GetMethod("Remove");
                        method.Invoke(list, new object[] { result });
                    }
                }

                static void SetProperty(object instance, int lineCount, PropertyInfo property, string propertyStringValue)
                {
                    if (TryParse(lineCount, property.Name, property.PropertyType, propertyStringValue, out object result))
                    {
                        property.SetValue(instance, result);
                    }
                }

                static bool TryParse(int lineCount, string propertyName, Type propertyType, string propertyStringValue, out object result)
                {
                    result = null;

                    if (propertyType == typeof(string))
                    {
                        result = propertyStringValue;
                        return true;
                    }
                    else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                    {
                        if (bool.TryParse(propertyStringValue, out bool boolResult))
                        {
                            result = boolResult;
                            return true;
                        }

                        Console.WriteLine($"ERROR - Line {lineCount}: Unable to parse {propertyStringValue} for property {propertyName} as a bool!");
                        return false;
                    }
                    else if (propertyType == typeof(int) || propertyType == typeof(int?))
                    {
                        if (int.TryParse(propertyStringValue, out int intResult))
                        {
                            result = intResult;
                            return true;
                        }

                        Console.WriteLine($"ERROR - Line {lineCount}: Unable to parse {propertyStringValue} for property {propertyName} as an int!");
                        return false;
                    }
                    else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var enumType = propertyType.GetGenericArguments()[0];

                        if (Enum.TryParse(enumType, propertyStringValue, true, out object enumResult))
                        {
                            result = enumResult;
                            return true;
                        }

                        Console.WriteLine($"ERROR - Line {lineCount}: Unable to parse enum value {propertyStringValue} for property {propertyName} to type {enumType}!");
                        return false;
                    }
                    else if (propertyType.IsEnum)
                    {
                        if (Enum.TryParse(propertyType, propertyStringValue, true, out object enumResult))
                        {
                            result = enumResult;
                            return true;
                        }

                        Console.WriteLine($"ERROR - Line {lineCount}: Unable to parse enum value {propertyStringValue} for property {propertyName} to type {propertyType}!");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"ERROR - Line {lineCount}: Can't parse property {propertyName} of type {propertyType.FullName} yet!");
                        return false;
                    }
                }
            }

            return objects.Values.ToList();
        }

        public static string Combine(List<string> iniStrings)
        {
            if (iniStrings == null || iniStrings.Count == 0)
            {
                return null;
            }

            var typesToLines = new Dictionary<string, List<string>>();
            List<string> currentTypeLines = null;
            string currentTypeName = null;

            for (int i = 0; i < iniStrings.Count; i++)
            {
                foreach (Line line in GetLines(iniStrings[i]))
                {
                    if (IsTypeLine(line, out string typeName))
                    {
                        currentTypeName = typeName;

                        if (typesToLines.TryGetValue(currentTypeName, out currentTypeLines) == false)
                        {
                            currentTypeLines = new List<string>();
                            typesToLines[currentTypeName] = currentTypeLines;
                        }

                        continue;
                    }

                    if (IsValidPropertyLine(line, out int equalsIndex) == false)
                    {
                        continue;
                    }

                    string propertyName = line.Value.Substring(0, equalsIndex).Trim();
                    string propertyNameWithEquals = propertyName + "=";
                    string propertyStringValue = line.Value.Substring(equalsIndex + 1).Trim();
                    bool isListAdd = propertyName.StartsWith("+");
                    bool isListRemove = propertyName.StartsWith("-");

                    if (isListAdd)
                    {
                        // Find the index of the most recent property and insert this item after it
                        int index = currentTypeLines.FindLastIndex(x => x.StartsWith(propertyNameWithEquals));

                        if (index != -1)
                        {
                            currentTypeLines.Insert(index + 1, line.Value);
                        }
                        else
                        {
                            currentTypeLines.Add(line.Value);
                        }
                    }
                    else if (isListRemove)
                    {
                        // Removing an array item if it exists in the list
                        string itemToRemove = "+" + line.Value.Substring(1);
                        int index = currentTypeLines.IndexOf(itemToRemove);

                        if (index != -1)
                        {
                            currentTypeLines.RemoveAt(index);
                        }
                    }
                    else
                    {
                        // Does this property already exist in the list? If so, overwrtie the old one, else, append it
                        int index = currentTypeLines.FindIndex(x => x.StartsWith(propertyNameWithEquals));

                        if (index != -1)
                        {
                            currentTypeLines[index] = line.Value;
                        }
                        else
                        {
                            currentTypeLines.Add(line.Value);
                        }
                    }
                }
            }

            // Constructing the file output
            var result = new StringBuilder();

            foreach (var type in typesToLines)
            {
                result.AppendLine($"[{type.Key}]");

                foreach (var line in type.Value)
                {
                    result.AppendLine(line);
                }

                result.AppendLine();
            }

            return result.ToString();
        }

        private static bool IsValidPropertyLine(Line line, out int equalsIndex)
        {
            // Making sure line contains an equals
            equalsIndex = line.Value.IndexOf('=');

            if (equalsIndex == -1)
            {
                Console.WriteLine($"ERROR - Line {line.Nunber}: Property line does not contain '=' character!");
                return false; ;
            }

            return true;
        }

        private static bool IsTypeLine(Line line, out string typeName)
        {
            // Detecting if a class type has been specified
            if (line.Value.StartsWith("["))
            {
                typeName = line.Value
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Trim();

                return true;
            }

            typeName = null;
            return false;
        }

        private static IEnumerable<Line> GetLines(string iniString)
        {
            if (iniString == null)
            {
                yield break;
            }

            using (var reader = new StringReader(iniString))
            {
                string line = null;

                int lineCount = -1;
                while ((line = reader.ReadLine()) != null)
                {
                    lineCount++;
                    line = line.Trim();

                    // Ignore empty lines and comments
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
                    {
                        continue;
                    }

                    yield return new Line { Nunber = lineCount, Value = line };
                }
            }
        }

        private struct Line
        {
            public int Nunber;
            public string Value;
        }
    }
}
