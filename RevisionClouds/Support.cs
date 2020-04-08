#region License
/*Данный код опубликован под лицензией Creative Commons Attribution-NonCommercial-ShareAlike.
Разрешено использовать, распространять, изменять и брать данный код за основу для производных в некоммерческих целях,
при условии указания авторства и если производные лицензируются на тех же условиях.
Код поставляется "как есть". Автор не несет ответственности за возможные последствия использования.
Зуев Александр, 2020, все права защищены.
This code is listed under the Creative Commons Attribution-NonCommercial-ShareAlike license.
You may use, redistribute, remix, tweak, and build upon this work non-commercially,
as long as you credit the author by linking back and license your new creations under the same terms.
This code is provided 'as is'. Author disclaims any implied warranty.
Zuev Aleksandr, 2020, all rigths reserved.*/
#endregion
#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
#endregion

namespace RevisionClouds
{
    public static class Support
    {
        /// <summary>
        /// Возвращает массив из 4 строк, с количеством облаков на листе из последних 4 ревизий
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public static string[] GetCloudsCountOnSheet(ViewSheet sheet)
        {
            Document doc = sheet.Document;
            List<RevisionCloud> clouds = new FilteredElementCollector(doc)
                .OfClass(typeof(RevisionCloud))
                .Cast<RevisionCloud>()
                .Where(i => i.OwnerViewId.IntegerValue == sheet.Id.IntegerValue)
                .ToList();

            List<Revision> allRevisionsOnSheet = sheet.GetAllRevisionIds()
                .Select(i => doc.GetElement(i))
                .Cast<Revision>()
                .OrderBy(i => Settings.revisionParam.GetValueFromRevision(i))
                .ToList();

            var allRevisionsGrouped = allRevisionsOnSheet.GroupBy(i => Settings.revisionParam.GetValueFromRevision(i)).ToDictionary(j => j.Key, j => j.ToList());

            Dictionary<string, List<Revision>> revsBase = new Dictionary<string, List<Revision>>();
            foreach (Revision rev in allRevisionsOnSheet)
            {
                string utvDlya = Settings.revisionParam.GetValueFromRevision(rev);
            }

            int revisionsCount = allRevisionsOnSheet.Count;
            List<Revision> lastRevisionsOnSheet = new List<Revision>();

            if (revisionsCount > 4)
            {
                lastRevisionsOnSheet = allRevisionsOnSheet.GetRange(revisionsCount - 4, 4);
            }
            else
            {
                lastRevisionsOnSheet = allRevisionsOnSheet;
            }

            int revsCount = lastRevisionsOnSheet.Count;
            string[] cloudsCounts = new string[revsCount];

            for (int i = 0; i < revsCount; i++)
            {
                Revision rev = lastRevisionsOnSheet[i];
                int curRevCloudsCount = clouds.Where(c => c.RevisionId.IntegerValue == rev.Id.IntegerValue).ToList().Count;
                string val = "";
                if (curRevCloudsCount == 0) val = "-";
                else val = curRevCloudsCount.ToString();

                cloudsCounts[i] = val;
            }

            return cloudsCounts;
        }

        /// <summary>
        /// Возвращает строку, где подряд идущие числа из массива заменены на прочерк, и перечислены через запятую
        /// </summary>
        /// <param name="marksString"></param>
        /// <returns></returns>
        public static string GetMarksRange(List<string> marksString)
        {
            List<int> marks = marksString
                .Select(i => Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(i, @"[^\d]+", "")))
                .ToList();

            if (marks.Count == 1) return marks[0].ToString();

            string range = marks[0].ToString();
            if (marks[1] == marks[0] + 1) range += "-";
            if (marks[1] != marks[0] + 1) range += ", ";

            for (int i = 1; i < marks.Count - 1; i++)
            {
                int curMark = marks[i];
                if (marks[i + 1] != curMark + 1)
                {
                    range += curMark + ", ";
                    continue;
                }
                else if (marks[i - 1] != curMark - 1)
                {
                    range += curMark + "-";
                    continue;
                }
            }
            range += marks[marks.Count - 1];

            return range;
        }

    }
}
