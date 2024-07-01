#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB.Architecture;

#endregion

namespace FilteringPlugin
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            #region Get View Name And Filter Option
            FilteredElementCollector CollectedViews = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType();
            List<string> ViewNames = new List<string>();
            foreach(View View in CollectedViews)
            {
                if (!((View.ViewType == ViewType.SystemBrowser)|| (View.ViewType == ViewType.Walkthrough) || (View.ViewType == ViewType.DraftingView)
                    || (View.ViewType == ViewType.SystemsAnalysisReport) || (View.ViewType == ViewType.Walkthrough) || (View.ViewType == ViewType.Rendering)
                    || (View.ViewType == ViewType.ColumnSchedule) || (View.ViewType == ViewType.CostReport) || (View.ViewType == ViewType.Detail)
                    || (View.ViewType == ViewType.DraftingView) || (View.ViewType == ViewType.DrawingSheet) || (View.ViewType == ViewType.Internal)
                    || (View.ViewType == ViewType.Legend) || (View.ViewType == ViewType.LoadsReport) || (View.ViewType == ViewType.PanelSchedule)
                    || (View.ViewType == ViewType.PresureLossReport) || (View.ViewType == ViewType.ProjectBrowser) || (View.ViewType == ViewType.Report)
                    || (View.ViewType == ViewType.Schedule) || (View.ViewType == ViewType.SystemsAnalysisReport) || (View.ViewType == ViewType.Undefined)))
                {
                    ViewNames.Add(View.Title);
                }
                
            }
            #region Open Window
            ChooseView x = new ChooseView(ViewNames);
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            x.Height = 365;
            x.Width = 510;
            double windowWidth = x.Width;
            double windowHeight = x.Height;
            x.Left = (screenWidth / 2) - (windowWidth / 2);
            x.Top = (screenHeight / 2) - (windowHeight / 2);
            x.ShowDialog();
            #endregion
            var Views = x.ViewName;
            string FilterOption = x.Filter;
            bool Continue = x.Continue;
            bool OverrideLine = x.OverrideLine;
            #endregion

            if (Continue)
            {
                #region Get All Colors
                List<System.Drawing.Color> colorList = ColorStructToList();
                #endregion

                #region Get Lists
                #region Get List Of Categories
                Categories categories = doc.Settings.Categories;
                Dictionary<string,BuiltInCategory> myCategories =new Dictionary<string,BuiltInCategory>();
                foreach (Category c in categories)
                {
                    BuiltInCategory enumCategory = (BuiltInCategory)c.Id.IntegerValue;
                    myCategories.Add(c.Name,enumCategory);
                }
                #endregion
                #region Get List Of Families
                /* FilteredElementCollector collectorFamily = new FilteredElementCollector(doc);
                 ICollection<Element> Familyelements = collectorFamily.OfClass(typeof(Family)).ToElements();
                 List<string> FamilyNames = new List<string>();
                 foreach(Element element in Familyelements)
                 {
                     FamilyNames.Add(element.Name);
                 }*/
                var Elements = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToList();
                Dictionary<string,List<Element>> ElementsByFamily = new Dictionary<string,List<Element>>();
                List<string> ElementsByName = new List<string>();
                foreach (var element in Elements) 
                {                
                    if(element is FamilyInstance)
                {
                        FamilyInstance FN = element as FamilyInstance;
                        FamilySymbol Sym = FN.Symbol;
                        string ModelFamName = Sym.FamilyName;
                        if (!(ElementsByFamily.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(element);
                            ElementsByFamily.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss=ElementsByFamily[ModelFamName];
                            ELementss.Add(element);
                            ElementsByFamily[ModelFamName] = ELementss;
                        }
                    }
                    else if (element is Wall)
                    {
                        WallType WT = (element as Wall).WallType;
                        string ModelFamName = WT.Category.Name;
                        if (!(ElementsByFamily.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(element);
                            ElementsByFamily.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByFamily[ModelFamName];
                            ELementss.Add(element);
                            ElementsByFamily[ModelFamName] = ELementss;
                        }
                    }
                    else if (element is Floor)
                    {
                        Floor Fo = element as Floor;
                        string ModelFamName = Fo.FloorType.FamilyName;
                        if (!(ElementsByFamily.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(element);
                            ElementsByFamily.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByFamily[ModelFamName];
                            ELementss.Add(element);
                            ElementsByFamily[ModelFamName] = ELementss;
                        }
                    }
                    else if (element is Ceiling)
                    {
                        Ceiling Fo = element as Ceiling;
                        CeilingType CeiType=doc.GetElement(Fo.GetTypeId()) as CeilingType;
                        string ModelFamName = CeiType.FamilyName;
                        if (!(ElementsByFamily.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(element);
                            ElementsByFamily.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByFamily[ModelFamName];
                            ELementss.Add(element);
                            ElementsByFamily[ModelFamName] = ELementss;
                        }
                    }
                    else if (element is MultistoryStairs)
                    {
                        string ModelFamName=element.GetType().Name;
                        var MS = doc.GetElement((element as MultistoryStairs).StandardStairsId);
                        if (!(ElementsByFamily.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(MS);
                            Elementss.Add(element);
                            ElementsByFamily.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByFamily[ModelFamName];
                            ELementss.Add(MS);
                            ELementss.Add(element);
                            ElementsByFamily[ModelFamName] = ELementss;
                        }
                    }
                }
                #endregion
                #region Get List OF Family Types
                /*FilteredElementCollector collectorTypes = new FilteredElementCollector(doc);
                ICollection<Element> Typeselements = collectorTypes.OfClass(typeof(FamilySymbol)).ToElements();
                List<string> TypeNames = new List<string>();
                foreach (Element element in Typeselements)
                {
                    TypeNames.Add(element.Name);
                }*/
                Dictionary<string, List<Element>> ElementsByType = new Dictionary<string, List<Element>>();
                foreach (var element in Elements)
                {
                    if (element is FamilyInstance)
                    {
                        FamilyInstance FN = element as FamilyInstance;
                        FamilySymbol Sym = FN.Symbol;
                        string ModelFamName = Sym.FamilyName+"__"+Sym.Name;
                        if (!(ElementsByType.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(element);
                            ElementsByType.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByType[ModelFamName];
                            ELementss.Add(element);
                            ElementsByType[ModelFamName] = ELementss;
                        }
                    }
                    else if (element is Wall)
                    {
                        WallType WT = (element as Wall).WallType;
                        string ModelFamName = WT.Category.Name+"__"+WT.Name;
                        if (!(ElementsByType.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(element);
                            ElementsByType.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByType[ModelFamName];
                            ELementss.Add(element);
                            ElementsByType[ModelFamName] = ELementss;
                        }
                    }
                    else if (element is Floor)
                    {
                        Floor Fo = element as Floor;
                        string ModelFamName = Fo.FloorType.FamilyName+"__"+Fo.FloorType.Name;
                        if (!(ElementsByType.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(element);
                            ElementsByType.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByType[ModelFamName];
                            ELementss.Add(element);
                            ElementsByType[ModelFamName] = ELementss;
                        }
                    }
                    else if (element is Ceiling)
                    {
                        Ceiling Fo = element as Ceiling;
                        CeilingType CeiType = doc.GetElement(Fo.GetTypeId()) as CeilingType;
                        string ModelFamName = CeiType.FamilyName + "__" +CeiType.Name;
                        if (!(ElementsByType.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(element);
                            ElementsByType.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByType[ModelFamName];
                            ELementss.Add(element);
                            ElementsByType[ModelFamName] = ELementss;
                        }
                    }
                    else if (element is MultistoryStairs)
                    {
                        string ModelFamName = element.GetType().Name;
                        var MS = doc.GetElement((element as MultistoryStairs).StandardStairsId);
                        if (!(ElementsByType.Keys.Contains(ModelFamName)))
                        {
                            List<Element> Elementss = new List<Element>();
                            Elementss.Add(MS);
                            Elementss.Add(element);
                            ElementsByType.Add(ModelFamName, Elementss);
                        }
                        else
                        {
                            var ELementss = ElementsByType[ModelFamName];
                            ELementss.Add(MS);
                            ELementss.Add(element);
                            ElementsByType[ModelFamName] = ELementss;
                        }
                    }
                }
                #endregion
                #endregion

                // Modify document within a transaction

                if(Views.Count > 0)
                {
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Created Filtered Views");
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                        Element solidFill = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Where(q => q.Name.Contains("Solid")).First();

                        View CreatedView = null;
                        #region Duplicate Views
                        foreach (var ViewName in Views)
                        {
                            View SelectedView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().Cast<View>().Where(m => m.Title == ViewName).FirstOrDefault();



                            try
                            {
                                #region Action
                                CreatedView = doc.GetElement(SelectedView.Duplicate(ViewDuplicateOption.Duplicate)) as View;


                                CreatedView.DisplayStyle = DisplayStyle.ShadingWithEdges;
                                CreatedView.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE).Set(4);

                                if (FilterOption == "Category")
                                {
                                    string Name = RemoveIllegalChara(SelectedView.Name);
                                    CreatedView.Name = "Colored By Category_" + Name;
                                    int CatCount = 0;
                                    foreach (string CategoryName in myCategories.Keys)
                                    {
                                        var ElementsList = new FilteredElementCollector(doc).OfCategory(myCategories[CategoryName]).WhereElementIsNotElementType().ToList();
                                        if (ElementsList.Count > 0)
                                        {
                                            if (CatCount >= colorList.Count - 1)
                                            {
                                                CatCount = 0;
                                            }
                                            var ElementColorSurface = new Autodesk.Revit.DB.Color(colorList[CatCount].R, colorList[CatCount].G, colorList[CatCount].B);
                                            CatCount = CatCount + 1;
                                            var ElementColorCut = new Autodesk.Revit.DB.Color(colorList[CatCount].R, colorList[CatCount].G, colorList[CatCount].B);
                                            foreach (Element element in ElementsList)
                                            {
                                                ogs.SetSurfaceBackgroundPatternId(solidFill.Id);
                                                ogs.SetSurfaceForegroundPatternId(solidFill.Id);
                                                ogs.SetCutBackgroundPatternId(solidFill.Id);
                                                ogs.SetCutForegroundPatternId(solidFill.Id);
                                                ogs.SetSurfaceBackgroundPatternColor(ElementColorSurface);
                                                ogs.SetSurfaceForegroundPatternColor(ElementColorSurface);
                                                ogs.SetCutBackgroundPatternColor(ElementColorCut);
                                                ogs.SetCutForegroundPatternColor(ElementColorCut);
                                                if (OverrideLine)
                                                {
                                                    ogs.SetCutLineColor(ElementColorCut);
                                                    ogs.SetProjectionLineColor(ElementColorSurface);
                                                }
                                                CreatedView.SetElementOverrides(element.Id, ogs);
                                            }
                                            CatCount++;
                                        }

                                    }
                                }

                                if (FilterOption == "Family")
                                {
                                    string Name = RemoveIllegalChara(SelectedView.Name);
                                    CreatedView.Name = "Colored By Family_" + Name;
                                    int CatCount = 0;
                                    foreach (string FamilyName in ElementsByFamily.Keys)
                                    {
                                        if (FamilyName == "MultistoryStairs")
                                        {

                                        }
                                        var ElementsList = ElementsByFamily[FamilyName];
                                        if (Elements.Count > 0)
                                        {
                                            if (CatCount >= colorList.Count - 1)
                                            {
                                                CatCount = 0;
                                            }
                                            var ElementColorSurface = new Autodesk.Revit.DB.Color(colorList[CatCount].R, colorList[CatCount].G, colorList[CatCount].B);
                                            CatCount = CatCount + 1;
                                            var ElementColorCut = new Autodesk.Revit.DB.Color(colorList[CatCount].R, colorList[CatCount].G, colorList[CatCount].B);
                                            foreach (Element element in ElementsList)
                                            {
                                                ogs.SetSurfaceBackgroundPatternId(solidFill.Id);
                                                ogs.SetSurfaceForegroundPatternId(solidFill.Id);
                                                ogs.SetCutBackgroundPatternId(solidFill.Id);
                                                ogs.SetCutForegroundPatternId(solidFill.Id);
                                                ogs.SetSurfaceBackgroundPatternColor(ElementColorSurface);
                                                ogs.SetSurfaceForegroundPatternColor(ElementColorSurface);
                                                ogs.SetCutBackgroundPatternColor(ElementColorCut);
                                                ogs.SetCutForegroundPatternColor(ElementColorCut);
                                                if (OverrideLine)
                                                {
                                                    ogs.SetCutLineColor(ElementColorCut);
                                                    ogs.SetProjectionLineColor(ElementColorSurface);
                                                }
                                                CreatedView.SetElementOverrides(element.Id, ogs);
                                            }
                                            CatCount++;
                                        }

                                    }
                                }

                                if (FilterOption == "Type")
                                {
                                    string Name = RemoveIllegalChara(SelectedView.Name);
                                    CreatedView.Name = "Colored By Type_" + Name;
                                    int CatCount = 0;
                                    foreach (string FamilyName in ElementsByType.Keys)
                                    {
                                        var ElementsList = ElementsByType[FamilyName];
                                        if (Elements.Count > 0)
                                        {
                                            if (CatCount >= colorList.Count - 1)
                                            {
                                                CatCount = 0;
                                            }
                                            var ElementColorSurface = new Autodesk.Revit.DB.Color(colorList[CatCount].R, colorList[CatCount].G, colorList[CatCount].B);
                                            CatCount = CatCount + 1;
                                            var ElementColorCut = new Autodesk.Revit.DB.Color(colorList[CatCount].R, colorList[CatCount].G, colorList[CatCount].B);
                                            foreach (Element element in ElementsList)
                                            {
                                                ogs.SetSurfaceBackgroundPatternId(solidFill.Id);
                                                ogs.SetSurfaceForegroundPatternId(solidFill.Id);
                                                ogs.SetCutBackgroundPatternId(solidFill.Id);
                                                ogs.SetCutForegroundPatternId(solidFill.Id);
                                                ogs.SetSurfaceBackgroundPatternColor(ElementColorSurface);
                                                ogs.SetSurfaceForegroundPatternColor(ElementColorSurface);
                                                ogs.SetCutBackgroundPatternColor(ElementColorCut);
                                                ogs.SetCutForegroundPatternColor(ElementColorCut);
                                                if (OverrideLine)
                                                {
                                                    ogs.SetCutLineColor(ElementColorCut);
                                                    ogs.SetProjectionLineColor(ElementColorSurface);
                                                }
                                                CreatedView.SetElementOverrides(element.Id, ogs);
                                            }
                                            CatCount++;
                                        }

                                    }
                                }
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        #endregion
                        #region Create 2D plan

                        /*var CheckView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().Where(m => m.Name == "Filtered View").FirstOrDefault();
                        bool PlanViewExist = true;
                        if (CheckView is null)
                        {
                            PlanViewExist = false;
                        }
                        int planCount = 0;
                        string PlanName= "Filtered View";
                        while (PlanViewExist)
                        {
                            PlanName = PlanName + planCount.ToString();
                            CheckView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().Where(m=>m.Name==PlanName).FirstOrDefault();
                            if (CheckView is null)
                            {
                                PlanViewExist = false;
                            }
                            planCount++;
                        }*/

                        #endregion
                        /*
                        #region Create 3D view
                        var viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(m => m.ViewFamily == ViewFamily.ThreeDimensional);
                        var Created3DView = View3D.CreateIsometric(doc, viewFamilyType.Id);
                        var CheckThreeView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().Where(m => m.Name == "Filtered 3D View").FirstOrDefault();
                        bool ThreeViewExist = true;
                        if (CheckThreeView is null)
                        {
                            ThreeViewExist = false;
                        }
                        int ThreeCount = 0;
                        string ThreeName = "Filtered 3D View";
                        while (ThreeViewExist)
                        {
                            ThreeName = ThreeName + ThreeCount.ToString();
                            CheckThreeView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().Where(m => m.Name == ThreeName).FirstOrDefault();
                            if (CheckThreeView is null)
                            {
                                ThreeViewExist = false;
                            }
                            ThreeCount++;
                        }
                        Created3DView.Name =ThreeName;
                        Created3DView.DisplayStyle = DisplayStyle.ShadingWithEdges;
                        Created3DView.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE).Set(4);
                        #endregion
                        */


                        tx.Commit();
                        uidoc.ActiveView = CreatedView;
                    }
                }
                else
                {
                    MessageBox.Show("Please choose a view","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                }


            
            }

            return Result.Succeeded;
        }

        public static List<System.Drawing.Color> ColorStructToList()
        {
            return typeof(System.Drawing.Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                                .Select(c => (System.Drawing.Color)c.GetValue(null, null))
                                .ToList();
        }

        public static string RemoveIllegalChara(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }
    }
}
