using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace Patch
{
    public class Patch
    {
        private static string ReadingFile = "Reading File - {0}";
        private static string KeyColumn = string.Empty;
        private static int origKeyCol = 0;
        private static int patchKeyCol = 0;

        private const string BEGINDATE = "BeginDate";
        private const string ENDDATE = "EndDate";
        private const string KEYCOLUMNMISMATCH = "Key Column Names doesn't match";
        private const string DUPLICATEKEYEXISTS = "Duplicate Keys Exists";
        private const string KEYCANNOTBEEMPTY = "Key cannot be empty string";
        private const string EMPTYKEYCOL = "Key Column Name cannot be empty";
        private const string INVALIDHEADERS = "Invalid Headers";
        private const string INVALIDARRAYINDEX = "Invalid Array Index";

        /// <summary>
        /// Compate Path Files.
        /// </summary>
        /// <param name="origFile">File Name with Path</param>
        /// <param name="patchFile">File Name with Path</param>
        public static void ComparePatchFiles(string origFile, string patchFile)
        {
            var origFileData = new List<List<string>>();
            var origHeaderRow = new List<string>();

            var patchFileData = new List<List<string>>();
            var patchheaderRow = new List<string>();

            Console.WriteLine(ReadingFile, origFile);
            using var origFileReader = new StreamReader(origFile);
            (origFileData, origHeaderRow) = LoadFileData(origFileReader);

            Console.WriteLine(ReadingFile, patchFile);
            using var patchFileReader = new StreamReader(patchFile);
            (patchFileData, patchheaderRow) = LoadFileData(patchFileReader);

            if (!CheckIfKeyColumnNameAreSame(origHeaderRow, patchheaderRow))
            {
                Console.WriteLine(KEYCOLUMNMISMATCH);
                return;
            }

            origKeyCol = origHeaderRow.IndexOf(KeyColumn);
            patchKeyCol = patchheaderRow.IndexOf(KeyColumn);

            if (DoesDuplicateKeyExists(origFileData, origHeaderRow) || DoesDuplicateKeyExists(patchFileData, patchheaderRow))
            {
                Console.WriteLine(DUPLICATEKEYEXISTS);
                return;
            }

            if (DoesEmptyKeyExists(origFileData, origHeaderRow) || DoesEmptyKeyExists(patchFileData, patchheaderRow))
            {
                Console.WriteLine(KEYCANNOTBEEMPTY);
                return;
            }

            var allKeys = GetAllKeys(origFileData, origHeaderRow, patchFileData, patchheaderRow);
            var allColHeaders = GetAllColumnHeaders(origHeaderRow, patchheaderRow);
            var isColumnChangesDetected = false;

            foreach (var key in allKeys)
            {
                var originalFileDataForKey = origFileData.Select(x => x).Where(x => x[origKeyCol] == key).FirstOrDefault();
                var patchFileDataForKey = patchFileData.Select(x => x).Where(x => x[patchKeyCol] == key).FirstOrDefault();

                if (!isColumnChangesDetected)
                    isColumnChangesDetected = DetectColumnChanges(key, allColHeaders, origHeaderRow, patchheaderRow, originalFileDataForKey, patchFileDataForKey);

                DetectColumnValueChanges(key, allColHeaders, origHeaderRow, patchheaderRow, originalFileDataForKey, patchFileDataForKey);
            }
        }

        /// <summary>
        /// Load Input File Data using CSV Parser
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static (List<List<string>>, List<string>) LoadFileData(TextReader reader)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", TrimOptions = TrimOptions.Trim };
            using var csvParser = new CsvParser(reader, csvConfig);
            csvParser.Read();
            var origHeaderRow = csvParser.Record.ToList();
            
            var origFileData = new List<List<string>>();
            var i = 1;
            while (csvParser.Read())
            {
                var row = csvParser.Record.ToList();
                origFileData.Add(row);
                i++;
            }
            return (origFileData, origHeaderRow); 
        }

        /// <summary>
        ///Should Work for
        ///BeginDate,EndDate,Issuer,Country,Conviction,Industry,Sector
        ///EndDate,Issuer,Country,Conviction,Industry,Sector,BeginDate
        ///BeginDate,Issuer,EndDate,Issuer,Country,Conviction,Industry,Sector
        ///Country,Conviction,Industry,Sector,BeginDate,EndDate,Issuer
        /// </summary>
        /// <param name="origHeaderRow"></param>
        /// <param name="patchHeaderRow"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static bool CheckIfKeyColumnNameAreSame(List<string> origHeaderRow, List<string> patchHeaderRow)
        {
            if (origHeaderRow.Count > 0 && patchHeaderRow.Count > 0)
            {
                var headerRow1KeyColumn = FindHeaderRowKeyCol(origHeaderRow);
                var headerRow2KeyColumn = FindHeaderRowKeyCol(patchHeaderRow);

                KeyColumn = headerRow1KeyColumn == headerRow2KeyColumn ? headerRow1KeyColumn : string.Empty;

                if (string.IsNullOrEmpty(headerRow1KeyColumn) || string.IsNullOrEmpty(headerRow2KeyColumn))
                {
                    Console.WriteLine(EMPTYKEYCOL);
                }
                return headerRow1KeyColumn.Equals(headerRow2KeyColumn);
            }
            else
            {
                throw new Exception(INVALIDHEADERS);
            }
        }

        /// <summary>
        /// Find Header Row Key Column
        /// </summary>
        /// <param name="headerRow"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string FindHeaderRowKeyCol(List<string> headerRow)
        {
            var headerRowKeyColumn = string.Empty;
            int beginDateIndex = headerRow.IndexOf(BEGINDATE);
            int endDateIndex = headerRow.IndexOf(ENDDATE);

            if (beginDateIndex != -1 && endDateIndex != -1)
            {
                if (beginDateIndex < endDateIndex)
                {
                    headerRowKeyColumn = GetNextElement(headerRow, beginDateIndex);
                    if (headerRowKeyColumn == ENDDATE)
                    {
                        headerRowKeyColumn = GetNextElement(headerRow, endDateIndex);
                    }
                }
                else
                {
                    headerRowKeyColumn = GetNextElement(headerRow, endDateIndex);
                    if (headerRowKeyColumn == BEGINDATE)
                    {
                        headerRowKeyColumn = GetNextElement(headerRow, beginDateIndex);
                    }
                }
            }
            else
            {
                throw new Exception(INVALIDHEADERS);
            }
            return headerRowKeyColumn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strList"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string GetNextElement(List<string> strList, int index)
        {
            if ((index > strList.Count - 1) || (index < 0))
                throw new Exception(INVALIDARRAYINDEX);

            else if (index == strList.Count - 1)
                index = 0;

            else
                index++;

            return strList[index];
        }

        /// <summary>
        /// Check if Duplicate Key value Exists in the file
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="headerRow"></param>
        /// <returns></returns>
        private static bool DoesDuplicateKeyExists(List<List<string>> fileData, List<string> headerRow)
        {
            var keyCol = headerRow.IndexOf(KeyColumn);
            var keyColData = fileData.Select(x => x[keyCol]).ToList();
            var result = keyColData.GroupBy(y => y).Select(g => new { Text = g.Key, Count = g.Count() }).OrderByDescending(j => j.Count);
            return result?.FirstOrDefault()?.Count > 1;
        }

        /// <summary>
        /// Keys cannot be empty for any rows in the csv file.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="headerRow"></param>
        /// <returns></returns>
        private static bool DoesEmptyKeyExists(List<List<string>> fileData, List<string> headerRow)
        {
            var keyCol = headerRow.IndexOf(KeyColumn);
            var keyColData = fileData.Select(x => x[keyCol]).Where(y => y.Equals(string.Empty)).ToList();
            var result = keyColData.GroupBy(y => y).Select(g => new { Text = g.Key, Count = g.Count() }).OrderByDescending(j => j.Count);
            return result?.FirstOrDefault()?.Count > 0;
        }

        /// <summary>
        /// Get All Keys from both the files.
        /// </summary>
        /// <param name="origFileData"></param>
        /// <param name="origHeaderRow"></param>
        /// <param name="patchFileData"></param>
        /// <param name="patchheaderRow"></param>
        /// <returns></returns>
        private static List<string> GetAllKeys(List<List<string>> origFileData, List<string> origHeaderRow, List<List<string>> patchFileData, List<string> patchheaderRow)
        {
            var origKeyColData = origFileData.Select(x => x[origKeyCol]).ToList();
            var patchKeyColData = patchFileData.Select(x => x[patchKeyCol]).ToList();
            return (origKeyColData.Concat(patchKeyColData).Distinct().ToList());
        }

        /// <summary>
        /// Get All Column Headers
        /// </summary>
        /// <param name="origHeaderRow"></param>
        /// <param name="patchHeaderRow"></param>
        /// <returns></returns>
        private static List<string> GetAllColumnHeaders(List<string> origHeaderRow, List<string> patchHeaderRow)
        {
            return origHeaderRow.Concat(patchHeaderRow).Distinct().ToList();
        }

        /// <summary>
        /// Detect Column Changes. Detect Added/Removed Columns
        /// </summary>
        /// <param name="key"></param>
        /// <param name="allColHeaders"></param>
        /// <param name="origHeaderRow"></param>
        /// <param name="patchheaderRow"></param>
        /// <param name="originalFileDataForKey"></param>
        /// <param name="patchFileDataForKey"></param>
        /// <returns></returns>
        private static bool DetectColumnChanges(string key, List<string> allColHeaders, List<string> origHeaderRow, List<string> patchheaderRow, List<string>? originalFileDataForKey, List<string>? patchFileDataForKey)
        {
            var sb = new StringBuilder();

            foreach (var col in allColHeaders)
            {
                var origColIndex = origHeaderRow.IndexOf(col);
                var patchColIndex = patchheaderRow.IndexOf(col);

                var origFileColVal = origColIndex == -1 ? null : originalFileDataForKey?[origColIndex];
                var patchFileColVal = patchColIndex == -1 ? null : patchFileDataForKey?[patchColIndex];

                if (origHeaderRow.IndexOf(col) >= 0 && patchFileColVal == null)
                {
                    sb.AppendFormat("Column {0}, removed from the new patch file.\n", col.ToString());
                }
                else if (origHeaderRow.IndexOf(col) == -1 && patchFileColVal != null)
                {
                    sb.AppendFormat("Column {0}, added to the new patch file.\n", col.ToString());
                }
            }
            Console.WriteLine(sb.ToString());
            return true;
        }

        /// <summary>
        /// Detect Column value Changes.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="allColHeaders"></param>
        /// <param name="origHeaderRow"></param>
        /// <param name="patchheaderRow"></param>
        /// <param name="originalFileDataForKey"></param>
        /// <param name="patchFileDataForKey"></param>
        private static void DetectColumnValueChanges(string key, List<string> allColHeaders, List<string> origHeaderRow, List<string> patchheaderRow, List<string>? originalFileDataForKey, List<string>? patchFileDataForKey)
        {
            var sb = new StringBuilder();

            foreach (var col in allColHeaders)
            {
                var origColIndex = origHeaderRow.IndexOf(col);
                var patchColIndex = patchheaderRow.IndexOf(col);

                var origFileColVal = origColIndex == -1 ? null : originalFileDataForKey?[origColIndex].ToString();
                var patchFileColVal = patchColIndex == -1 ? null : patchFileDataForKey?[patchColIndex].ToString();
                var isColRemoved = false;
                var isColAdded = false;

                if (origHeaderRow.IndexOf(col) >= 0 && patchFileColVal == null)
                {
                    //Column exists in original file, but removed from the new patch file.
                    isColRemoved = true;
                }
                else if (origHeaderRow.IndexOf(col) == -1 && patchFileColVal != null)
                {
                    //New Column Added to the new patch file.
                    isColAdded = true;
                }

                if (origFileColVal != patchFileColVal)
                {
                    if (!string.IsNullOrEmpty(origFileColVal) && !string.IsNullOrEmpty(patchFileColVal))
                    {
                        sb.AppendFormat("For {0} {1}, the {2} was changed from {3} to {4}.\n", KeyColumn, key, col, originalFileDataForKey?[origColIndex].ToString(), patchFileDataForKey?[patchColIndex].ToString());
                    }
                    else
                    {
                        if (!isColRemoved && !string.IsNullOrEmpty(patchFileColVal))
                            sb.AppendFormat("For {0} {1}, {2} was added as the {3}.\n", KeyColumn, key, patchFileColVal, col);
                    }
                }
            }
            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                Console.WriteLine(sb.ToString());
            }    
        }
    }
}
