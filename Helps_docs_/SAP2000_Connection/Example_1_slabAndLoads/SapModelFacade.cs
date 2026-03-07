// INSTRUCCIÓN PARA MANTENIMIENTO (NO ELIMINAR):
// Este archivo (`SapModelFacade`) se considera una *biblioteca/facade* reutilizable del API COM de SAP2000.
// Compilación condicional: SAP2000_AVAILABLE define si se compila con la referencia SAP2000v1.dll.
// En Release sin SAP2000, se usa el stub que lanza NotSupportedException.
using System;

#if SAP2000_AVAILABLE
using SAP2000v1;

namespace App.Infrastructure.Sap2000
{
    public sealed class SapModelFacade
    {
        private readonly cSapModel _sapModel;

    public SapModelFacade(cSapModel sapModel)
        {
   _sapModel = sapModel ?? throw new ArgumentNullException(nameof(sapModel));
     }

        public cSapModel Raw => _sapModel;

        public void Check(int ret, string where)
        {
            if (ret != 0) throw new InvalidOperationException($"{where} retornó código {ret}");
   }

      public void PropArea_SetShell(string name, int shellType, string material, double matAngle, double thickMembrane, double thickBending)
        {
 int r = _sapModel.PropArea.SetShell(name, shellType, material, matAngle, thickMembrane, thickBending);
     Check(r, $"PropArea.SetShell({name})");
     }

        public void AreaObj_AddByCoord(int nPts, ref double[] xs, ref double[] ys, ref double[] zs, ref string name)
        {
int r = _sapModel.AreaObj.AddByCoord(nPts, ref xs, ref ys, ref zs, ref name);
      Check(r, $"AreaObj.AddByCoord({name})");
        }

 public void AreaObj_ChangeName(string oldName, string newName)
        {
   int r = _sapModel.AreaObj.ChangeName(oldName, newName);
         Check(r, $"AreaObj.ChangeName({oldName}->{newName})");
        }

        public void AreaObj_SetProperty(string name, string prop)
        {
   int r = _sapModel.AreaObj.SetProperty(name, prop);
      Check(r, $"AreaObj.SetProperty({name}->{prop})");
     }

      public void EditArea_Divide(string areaName, int meshType, ref int numberAreas, ref string[] areaNames, int n1, int n2)
  {
      int r = _sapModel.EditArea.Divide(areaName, meshType, ref numberAreas, ref areaNames,
          n1, n2, 0.0, 0.0, false, false, false, false, 0.0, 0.0, false, false, false, false);
        Check(r, $"EditArea.Divide({areaName})");
        }

        public void LoadPatterns_Add(string name, eLoadPatternType type, double selfWtMult = 0, bool addAnalysisCase = true)
        {
    int r = _sapModel.LoadPatterns.Add(name, type, selfWtMult, addAnalysisCase);
    Check(r, $"LoadPatterns.Add({name})");
        }

        public void RespCombo_Add(string name, int comboType)
        {
      int r = _sapModel.RespCombo.Add(name, comboType);
     Check(r, $"RespCombo.Add({name})");
        }

        public void RespCombo_SetCaseList(string comboName, ref eCNameType caseType, string caseName, double scaleFactor)
        {
            int r = _sapModel.RespCombo.SetCaseList(comboName, ref caseType, caseName, scaleFactor);
    Check(r, $"RespCombo.SetCaseList({comboName}, {caseName})");
        }

        public void LoadCases_StaticLinear_SetCase(string name)
        {
     int r = _sapModel.LoadCases.StaticLinear.SetCase(name);
            Check(r, $"LoadCases.StaticLinear.SetCase({name})");
        }

        public void LoadCases_StaticLinear_SetInitialCase(string name, string initialCase)
        {
         int r = _sapModel.LoadCases.StaticLinear.SetInitialCase(name, initialCase);
            Check(r, $"LoadCases.StaticLinear.SetInitialCase({name}, {initialCase})");
        }

   public void LoadCases_StaticLinear_SetLoads(string name, int numberLoads, string[] loadType, string[] loadName, double[] sf)
        {
            int r = _sapModel.LoadCases.StaticLinear.SetLoads(name, numberLoads, ref loadType, ref loadName, ref sf);
   Check(r, $"LoadCases.StaticLinear.SetLoads({name})");
   }

    public void LoadCases_StaticNonlinear_SetCase(string name)
    {
     int r = _sapModel.LoadCases.StaticNonlinear.SetCase(name);
   Check(r, $"LoadCases.StaticNonlinear.SetCase({name})");
        }

        public void LoadCases_StaticNonlinear_SetInitialCase(string name, string initialCase)
      {
          int r = _sapModel.LoadCases.StaticNonlinear.SetInitialCase(name, initialCase);
          Check(r, $"LoadCases.StaticNonlinear.SetInitialCase({name}, {initialCase})");
        }

        public void LoadCases_StaticNonlinear_SetGeometricNonlinearity(string name, int nlGeomType)
    {
    int r = _sapModel.LoadCases.StaticNonlinear.SetGeometricNonlinearity(name, nlGeomType);
  Check(r, $"LoadCases.StaticNonlinear.SetGeometricNonlinearity({name}, {nlGeomType})");
        }

public void LoadCases_StaticNonlinear_SetLoadApplication(string name, int loadControl, int dispType, double displ, int monitor, int dof, string pointName, string gDispl)
        {
        int r = _sapModel.LoadCases.StaticNonlinear.SetLoadApplication(name, loadControl, dispType, displ, monitor, dof, pointName, gDispl);
     Check(r, $"LoadCases.StaticNonlinear.SetLoadApplication({name})");
        }

        public void LoadCases_StaticNonlinear_SetLoads(string name, int numberLoads, string[] loadType, string[] loadName, double[] sf)
        {
   int r = _sapModel.LoadCases.StaticNonlinear.SetLoads(name, numberLoads, ref loadType, ref loadName, ref sf);
    Check(r, $"LoadCases.StaticNonlinear.SetLoads({name})");
        }

        public void LoadCases_StaticNonlinear_SetMassSource(string name, string source)
        {
            int r = _sapModel.LoadCases.StaticNonlinear.SetMassSource(name, source);
  Check(r, $"LoadCases.StaticNonlinear.SetMassSource({name}, {source})");
        }

        public void LoadCases_StaticNonlinear_SetModalCase(string name, string modalCase)
        {
     int r = _sapModel.LoadCases.StaticNonlinear.SetModalCase(name, modalCase);
            Check(r, $"LoadCases.StaticNonlinear.SetModalCase({name}, {modalCase})");
        }

      public void LoadCases_StaticNonlinear_SetResultsSaved(string name, bool saveMultipleSteps, int minSavedStates = 10, int maxSavedStates = 100, bool positiveOnly = true)
        {
        int r = _sapModel.LoadCases.StaticNonlinear.SetResultsSaved(name, saveMultipleSteps, minSavedStates, maxSavedStates, positiveOnly);
          Check(r, $"LoadCases.StaticNonlinear.SetResultsSaved({name})");
    }

        public void LoadCases_StaticNonlinear_SetSolControlParameters(string name, int maxTotalSteps, int maxFailedSubSteps, int maxIterCS, int maxIterNR, double tolConvD, bool useEventStepping, double tolEventD, int maxLineSearchPerIter, double tolLineSearch, double lineSearchStepFact)
        {
          int r = _sapModel.LoadCases.StaticNonlinear.SetSolControlParameters(name, maxTotalSteps, maxFailedSubSteps, maxIterCS, maxIterNR, tolConvD, useEventStepping, tolEventD, maxLineSearchPerIter, tolLineSearch, lineSearchStepFact);
            Check(r, $"LoadCases.StaticNonlinear.SetSolControlParameters({name})");
 }

   public void LoadCases_StaticNonlinear_SetTargetForceParameters(string name, double tolConvF, int maxIter, double accelFact, bool noStop)
        {
            int r = _sapModel.LoadCases.StaticNonlinear.SetTargetForceParameters(name, tolConvF, maxIter, accelFact, noStop);
            Check(r, $"LoadCases.StaticNonlinear.SetTargetForceParameters({name})");
   }

        public void LoadCases_ModalEigen_SetCase(string name)
        {
 int r = _sapModel.LoadCases.ModalEigen.SetCase(name);
            Check(r, $"LoadCases.ModalEigen.SetCase({name})");
    }

     public void LoadCases_ModalEigen_SetInitialCase(string name, string initialCase)
  {
            int r = _sapModel.LoadCases.ModalEigen.SetInitialCase(name, initialCase);
    Check(r, $"LoadCases.ModalEigen.SetInitialCase({name}, {initialCase})");
        }

        public void LoadCases_ModalEigen_SetLoads(string name, int numberLoads, string[] loadType, string[] loadName, double[] targetPar, bool[] staticCorrect)
 {
            int r = _sapModel.LoadCases.ModalEigen.SetLoads(name, numberLoads, ref loadType, ref loadName, ref targetPar, ref staticCorrect);
   Check(r, $"LoadCases.ModalEigen.SetLoads({name})");
        }

    public void LoadCases_ModalEigen_SetNumberModes(string name, int maxModes, int minModes)
 {
            int r = _sapModel.LoadCases.ModalEigen.SetNumberModes(name, maxModes, minModes);
            Check(r, $"LoadCases.ModalEigen.SetNumberModes({name}, {maxModes}, {minModes})");
      }

        public void LoadCases_ModalEigen_SetParameters(string name, double eigenShiftFreq, double eigenCutOff, double eigenTol, int allowAutoFreqShift)
        {
   int r = _sapModel.LoadCases.ModalEigen.SetParameters(name, eigenShiftFreq, eigenCutOff, eigenTol, allowAutoFreqShift);
            Check(r, $"LoadCases.ModalEigen.SetParameters({name})");
        }

        public void LoadCases_ModalRitz_SetCase(string name)
 {
int r = _sapModel.LoadCases.ModalRitz.SetCase(name);
            Check(r, $"LoadCases.ModalRitz.SetCase({name})");
        }

      public void LoadCases_ModalRitz_SetInitialCase(string name, string initialCase)
        {
        int r = _sapModel.LoadCases.ModalRitz.SetInitialCase(name, initialCase);
            Check(r, $"LoadCases.ModalRitz.SetInitialCase({name}, {initialCase})");
}

        public void LoadCases_ModalRitz_SetLoads(string name, int numberLoads, string[] loadType, string[] loadName, int[] ritzMaxCyc, double[] targetPar)
    {
     int r = _sapModel.LoadCases.ModalRitz.SetLoads(name, numberLoads, ref loadType, ref loadName, ref ritzMaxCyc, ref targetPar);
            Check(r, $"LoadCases.ModalRitz.SetLoads({name})");
        }

        public void LoadCases_ModalRitz_SetNumberModes(string name, int maxModes, int minModes)
        {
  int r = _sapModel.LoadCases.ModalRitz.SetNumberModes(name, maxModes, minModes);
Check(r, $"LoadCases.ModalRitz.SetNumberModes({name}, {maxModes}, {minModes})");
        }

    public void LoadCases_ResponseSpectrum_SetCase(string name)
        {
     int r = _sapModel.LoadCases.ResponseSpectrum.SetCase(name);
         Check(r, $"LoadCases.ResponseSpectrum.SetCase({name})");
        }

        public void LoadCases_ResponseSpectrum_SetDampConstant(string name, double damp)
    {
      int r = _sapModel.LoadCases.ResponseSpectrum.SetDampConstant(name, damp);
            Check(r, $"LoadCases.ResponseSpectrum.SetDampConstant({name})");
        }

        public void LoadCases_ResponseSpectrum_SetDampInterpolated(string name, int dampType, int numberItems, double[] time, double[] damp)
        {
            int r = _sapModel.LoadCases.ResponseSpectrum.SetDampInterpolated(name, dampType, numberItems, ref time, ref damp);
            Check(r, $"LoadCases.ResponseSpectrum.SetDampInterpolated({name})");
        }

        public void LoadCases_ResponseSpectrum_SetDampOverrides(string name, int numberItems, int[] mode, double[] damp)
        {
         int r = _sapModel.LoadCases.ResponseSpectrum.SetDampOverrides(name, numberItems, ref mode, ref damp);
  Check(r, $"LoadCases.ResponseSpectrum.SetDampOverrides({name})");
    }

        public void LoadCases_ResponseSpectrum_SetDampProportional(string name, int dampType, double dampa, double dampb, double dampf1, double dampf2, double dampd1, double dampd2)
        {
    int r = _sapModel.LoadCases.ResponseSpectrum.SetDampProportional(name, dampType, dampa, dampb, dampf1, dampf2, dampd1, dampd2);
  Check(r, $"LoadCases.ResponseSpectrum.SetDampProportional({name})");
        }

        public void LoadCases_ResponseSpectrum_SetDiaphragmEccentricityOverride(string name, string diaph, double eccen, bool delete = false)
     {
    int r = _sapModel.LoadCases.ResponseSpectrum.SetDiaphragmEccentricityOverride(name, diaph, eccen, delete);
            Check(r, $"LoadCases.ResponseSpectrum.SetDiaphragmEccentricityOverride({name}, {diaph})");
        }

        public void LoadCases_ResponseSpectrum_SetDirComb(string name, int myType, double sf)
    {
        int r = _sapModel.LoadCases.ResponseSpectrum.SetDirComb(name, myType, sf);
       Check(r, $"LoadCases.ResponseSpectrum.SetDirComb({name})");
        }

        public void LoadCases_ResponseSpectrum_SetEccentricity(string name, double eccen)
        {
         int r = _sapModel.LoadCases.ResponseSpectrum.SetEccentricity(name, eccen);
            Check(r, $"LoadCases.ResponseSpectrum.SetEccentricity({name})");
     }

  public void LoadCases_ResponseSpectrum_SetLoads(string name, int numberLoads, string[] loadName, string[] func, double[] sf, string[] cSys, double[] ang)
        {
     int r = _sapModel.LoadCases.ResponseSpectrum.SetLoads(name, numberLoads, ref loadName, ref func, ref sf, ref cSys, ref ang);
Check(r, $"LoadCases.ResponseSpectrum.SetLoads({name})");
        }

        public void LoadCases_ResponseSpectrum_SetModalCase(string name, string modalCase)
        {
 int r = _sapModel.LoadCases.ResponseSpectrum.SetModalCase(name, modalCase);
 Check(r, $"LoadCases.ResponseSpectrum.SetModalCase({name}, {modalCase})");
 }

      public void LoadCases_ResponseSpectrum_SetModalComb_1(string name, int myType, double f1 = 1, double f2 = 0, int periodicRigidCombType = 1, double td = 60)
        {
 int r = _sapModel.LoadCases.ResponseSpectrum.SetModalComb_1(name, myType, f1, f2, periodicRigidCombType, td);
    Check(r, $"LoadCases.ResponseSpectrum.SetModalComb_1({name})");
      }

        public string PointObj_AddCartesian(double x, double y, double z, string userName = "", string cSys = "Global")
        {
string name = string.Empty;
            int r = _sapModel.PointObj.AddCartesian(x, y, z, ref name, userName ?? string.Empty, cSys ?? "Global");
            Check(r, $"PointObj.AddCartesian({userName})");
            return name;
   }

  public void PointObj_SetSpecialPoint(string name, bool specialPoint, int itemType = 0)
    {
     int r = _sapModel.PointObj.SetSpecialPoint(name, specialPoint, (eItemType)itemType);
 Check(r, $"PointObj.SetSpecialPoint({name})");
        }

        public void PointObj_SetRestraint(string name, ref bool[] value, eItemType itemType)
 {
            int r = _sapModel.PointObj.SetRestraint(name, ref value, itemType);
            Check(r, $"PointObj.SetRestraint({name}, {itemType})");
        }

        public void PointObj_SetSelected(string name, bool selected, eItemType itemType = (eItemType)0)
{
            int r = _sapModel.PointObj.SetSelected(name, selected, itemType);
  Check(r, $"PointObj.SetSelected({name})");
   }

        public void PointObj_SetGroupAssign(string name, string groupName, bool remove, eItemType itemType)
        {
int r = _sapModel.PointObj.SetGroupAssign(name, groupName, remove, itemType);
          Check(r, $"PointObj.SetGroupAssign({name}->{groupName})");
        }

        public void ConstraintDef_SetBody(string name, ref bool[] value, string cSys = "Global")
 {
            int r = _sapModel.ConstraintDef.SetBody(name, ref value, cSys ?? "Global");
            Check(r, $"ConstraintDef.SetBody({name})");
 }

      public void PointObj_SetConstraint(string name, string constraintName, eItemType itemType, bool replace = true)
        {
            var cName = constraintName ?? string.Empty;
        int r = _sapModel.PointObj.SetConstraint(name, ref cName, itemType, replace);
      Check(r, $"PointObj.SetConstraint({name}->{cName}, {itemType})");
        }

      public void GroupDef_SetGroup(string name, int color = -1, bool specifiedForSelection = true)
        {
            int r = _sapModel.GroupDef.SetGroup(name, color, specifiedForSelection);
            Check(r, $"GroupDef.SetGroup({name})");
        }

        public string AreaObj_AddByPoint(int numberPoints, string[] points, string propName = "Default", string userName = "")
 {
  string name = string.Empty;
       int r = _sapModel.AreaObj.AddByPoint(numberPoints, ref points, ref name, propName, userName);
            Check(r, $"AreaObj.AddByPoint({userName})");
 return name;
    }

        public void AreaObj_SetGroupAssign(string areaName, string groupName, bool remove = false)
        {
    int r = _sapModel.AreaObj.SetGroupAssign(areaName, groupName, remove);
    Check(r, $"AreaObj.SetGroupAssign({areaName}->{groupName})");
        }

        public void PropMaterial_SetMaterial(string name, int matType)
  {
          int r = _sapModel.PropMaterial.SetMaterial(name, (eMatType)matType, -1, "", "");
   Check(r, $"PropMaterial.SetMaterial({name})");
        }

     public void PropMaterial_SetMPIsotropic(string name, double e, double u, double a)
   {
       int r = _sapModel.PropMaterial.SetMPIsotropic(name, e, u, a);
            Check(r, $"PropMaterial.SetMPIsotropic({name})");
        }

        public void PropMaterial_SetOConcrete_1(string name, double fc, bool isLightweight = false, double fcsFactor = 1.0, int ssType = 2, int ssHysType = 2, double strainAtFc = 0.00192, double strainUltimate = 0.005, double finalSlope = -0.10)
        {
 int r = _sapModel.PropMaterial.SetOConcrete_1(name, fc, isLightweight, fcsFactor, ssType, ssHysType, strainAtFc, strainUltimate, finalSlope);
         Check(r, $"PropMaterial.SetOConcrete_1({name})");
}

        public void SelectObj_ClearSelection()
        {
      int r = _sapModel.SelectObj.ClearSelection();
            Check(r, "SelectObj.ClearSelection");
        }

public void SelectObj_All(bool deselect = false)
        {
     int r = _sapModel.SelectObj.All(deselect);
  Check(r, $"SelectObj.All(deselect={deselect})");
        }

  public void SelectObj_PropertyArea(string propName, bool deselect = false)
{
        int r = _sapModel.SelectObj.PropertyArea(propName, deselect);
        Check(r, $"SelectObj.PropertyArea({propName}, deselect={deselect})");
        }

        public void AreaObj_SetLoadUniform(string name, string loadPat, double value, int dir = 10, bool replace = true, string cSys = "Global", eItemType itemType = (eItemType)0)
        {
      int r = _sapModel.AreaObj.SetLoadUniform(name, loadPat, value, dir, replace, cSys, itemType);
            Check(r, $"AreaObj.SetLoadUniform({name}, {loadPat}, {value})");
   }

        public void AreaObj_SetSpring(string name, int myType, double s, int simpleSpringType, string linkProp, int face, int springLocalOneType, int dir, bool outward, ref double[] vec, double ang, bool replace, string cSys, eItemType itemType)
        {
 int r = _sapModel.AreaObj.SetSpring(name, myType, s, simpleSpringType, linkProp, face, springLocalOneType, dir, outward, ref vec, ang, replace, cSys, itemType);
   Check(r, "AreaObj.SetSpring");
        }

        public void SapObject_Hide()
 {
          try
    {
       var sapObject = _sapModel.GetType().InvokeMember("SapObject", System.Reflection.BindingFlags.GetProperty, null, _sapModel, null);
     sapObject?.GetType().InvokeMember("Hide", System.Reflection.BindingFlags.InvokeMethod, null, sapObject, null);
}
    catch { }
        }

        public void SapObject_Unhide()
 {
 try
            {
  var sapObject = _sapModel.GetType().InvokeMember("SapObject", System.Reflection.BindingFlags.GetProperty, null, _sapModel, null);
   sapObject?.GetType().InvokeMember("Unhide", System.Reflection.BindingFlags.InvokeMethod, null, sapObject, null);
            }
         catch { }
        }

        public void View_RefreshView(int window = 0, bool zoom = true)
      {
            int r = _sapModel.View.RefreshView(window, zoom);
         Check(r, "View.RefreshView");
        }
  }
}
#else
// STUB VERSION when SAP2000_AVAILABLE is not defined
namespace App.Infrastructure.Sap2000
{
    public sealed class SapModelFacade
    {
public SapModelFacade(object sapModel) { }
        public object Raw => null;
        public void Check(int ret, string where) => throw new System.NotSupportedException("SAP2000 not available.");
  public void PropArea_SetShell(string name, int shellType, string material, double matAngle, double thickMembrane, double thickBending) => throw new System.NotSupportedException("SAP2000 not available.");
      public void AreaObj_AddByCoord(int nPts, ref double[] xs, ref double[] ys, ref double[] zs, ref string name) => throw new System.NotSupportedException("SAP2000 not available.");
        public void AreaObj_ChangeName(string oldName, string newName) => throw new System.NotSupportedException("SAP2000 not available.");
   public void AreaObj_SetProperty(string name, string prop) => throw new System.NotSupportedException("SAP2000 not available.");
        public void EditArea_Divide(string areaName, int meshType, ref int numberAreas, ref string[] areaNames, int n1, int n2) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadPatterns_Add(string name, int type, double selfWtMult = 0, bool addAnalysisCase = true) => throw new System.NotSupportedException("SAP2000 not available.");
        public void RespCombo_Add(string name, int comboType) => throw new System.NotSupportedException("SAP2000 not available.");
     public void RespCombo_SetCaseList(string comboName, int caseType, string caseName, double scaleFactor) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticLinear_SetCase(string name) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticLinear_SetInitialCase(string name, string initialCase) => throw new System.NotSupportedException("SAP2000 not available.");
  public void LoadCases_StaticLinear_SetLoads(string name, int numberLoads, string[] loadType, string[] loadName, double[] sf) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticNonlinear_SetCase(string name) => throw new System.NotSupportedException("SAP2000 not available.");
 public void LoadCases_StaticNonlinear_SetInitialCase(string name, string initialCase) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticNonlinear_SetGeometricNonlinearity(string name, int nlGeomType) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticNonlinear_SetLoadApplication(string name, int loadControl, int dispType, double displ, int monitor, int dof, string pointName, string gDispl) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticNonlinear_SetLoads(string name, int numberLoads, string[] loadType, string[] loadName, double[] sf) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticNonlinear_SetMassSource(string name, string source) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticNonlinear_SetModalCase(string name, string modalCase) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticNonlinear_SetResultsSaved(string name, bool saveMultipleSteps, int minSavedStates = 10, int maxSavedStates = 100, bool positiveOnly = true) => throw new System.NotSupportedException("SAP2000 not available.");
      public void LoadCases_StaticNonlinear_SetSolControlParameters(string name, int maxTotalSteps, int maxFailedSubSteps, int maxIterCS, int maxIterNR, double tolConvD, bool useEventStepping, double tolEventD, int maxLineSearchPerIter, double tolLineSearch, double lineSearchStepFact) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_StaticNonlinear_SetTargetForceParameters(string name, double tolConvF, int maxIter, double accelFact, bool noStop) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ModalEigen_SetCase(string name) => throw new System.NotSupportedException("SAP2000 not available.");
     public void LoadCases_ModalEigen_SetInitialCase(string name, string initialCase) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ModalEigen_SetLoads(string name, int numberLoads, string[] loadType, string[] loadName, double[] targetPar, bool[] staticCorrect) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ModalEigen_SetNumberModes(string name, int maxModes, int minModes) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ModalEigen_SetParameters(string name, double eigenShiftFreq, double eigenCutOff, double eigenTol, int allowAutoFreqShift) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ModalRitz_SetCase(string name) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ModalRitz_SetInitialCase(string name, string initialCase) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ModalRitz_SetLoads(string name, int numberLoads, string[] loadType, string[] loadName, int[] ritzMaxCyc, double[] targetPar) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ModalRitz_SetNumberModes(string name, int maxModes, int minModes) => throw new System.NotSupportedException("SAP2000 not available.");
  public void LoadCases_ResponseSpectrum_SetCase(string name) => throw new System.NotSupportedException("SAP2000 not available.");
   public void LoadCases_ResponseSpectrum_SetDampConstant(string name, double damp) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ResponseSpectrum_SetDampInterpolated(string name, int dampType, int numberItems, double[] time, double[] damp) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ResponseSpectrum_SetDampOverrides(string name, int numberItems, int[] mode, double[] damp) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ResponseSpectrum_SetDampProportional(string name, int dampType, double dampa, double dampb, double dampf1, double dampf2, double dampd1, double dampd2) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ResponseSpectrum_SetDiaphragmEccentricityOverride(string name, string diaph, double eccen, bool delete = false) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ResponseSpectrum_SetDirComb(string name, int myType, double sf) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ResponseSpectrum_SetEccentricity(string name, double eccen) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ResponseSpectrum_SetLoads(string name, int numberLoads, string[] loadName, string[] func, double[] sf, string[] cSys, double[] ang) => throw new System.NotSupportedException("SAP2000 not available.");
        public void LoadCases_ResponseSpectrum_SetModalCase(string name, string modalCase) => throw new System.NotSupportedException("SAP2000 not available.");
    public void LoadCases_ResponseSpectrum_SetModalComb_1(string name, int myType, double f1 = 1, double f2 = 0, int periodicRigidCombType = 1, double td = 60) => throw new System.NotSupportedException("SAP2000 not available.");
        public string PointObj_AddCartesian(double x, double y, double z, string userName = "", string cSys = "Global") => throw new System.NotSupportedException("SAP2000 not available.");
        public void PointObj_SetSpecialPoint(string name, bool specialPoint, int itemType = 0) => throw new System.NotSupportedException("SAP2000 not available.");
        public void PointObj_SetRestraint(string name, ref bool[] value, int itemType) => throw new System.NotSupportedException("SAP2000 not available.");
        public void PointObj_SetSelected(string name, bool selected, int itemType = 0) => throw new System.NotSupportedException("SAP2000 not available.");
        public void PointObj_SetGroupAssign(string name, string groupName, bool remove, int itemType) => throw new System.NotSupportedException("SAP2000 not available.");
        public void ConstraintDef_SetBody(string name, ref bool[] value, string cSys = "Global") => throw new System.NotSupportedException("SAP2000 not available.");
      public void PointObj_SetConstraint(string name, string constraintName, int itemType, bool replace = true) => throw new System.NotSupportedException("SAP2000 not available.");
        public void GroupDef_SetGroup(string name, int color = -1, bool specifiedForSelection = true) => throw new System.NotSupportedException("SAP2000 not available.");
        public string AreaObj_AddByPoint(int numberPoints, string[] points, string propName = "Default", string userName = "") => throw new System.NotSupportedException("SAP2000 not available.");
   public void AreaObj_SetGroupAssign(string areaName, string groupName, bool remove = false) => throw new System.NotSupportedException("SAP2000 not available.");
        public void PropMaterial_SetMaterial(string name, int matType) => throw new System.NotSupportedException("SAP2000 not available.");
        public void PropMaterial_SetMPIsotropic(string name, double e, double u, double a) => throw new System.NotSupportedException("SAP2000 not available.");
        public void PropMaterial_SetOConcrete_1(string name, double fc, bool isLightweight = false, double fcsFactor = 1.0, int ssType = 2, int ssHysType = 2, double strainAtFc = 0.00192, double strainUltimate = 0.005, double finalSlope = -0.10) => throw new System.NotSupportedException("SAP2000 not available.");
        public void SelectObj_ClearSelection() => throw new System.NotSupportedException("SAP2000 not available.");
     public void SelectObj_All(bool deselect = false) => throw new System.NotSupportedException("SAP2000 not available.");
        public void SelectObj_PropertyArea(string propName, bool deselect = false) => throw new System.NotSupportedException("SAP2000 not available.");
        public void AreaObj_SetLoadUniform(string name, string loadPat, double value, int dir = 10, bool replace = true, string cSys = "Global", int itemType = 0) => throw new System.NotSupportedException("SAP2000 not available.");
        public void AreaObj_SetSpring(string name, int myType, double s, int simpleSpringType, string linkProp, int face, int springLocalOneType, int dir, bool outward, ref double[] vec, double ang, bool replace, string cSys, int itemType) => throw new System.NotSupportedException("SAP2000 not available.");
        public void SapObject_Hide() { }
        public void SapObject_Unhide() { }
      public void View_RefreshView(int window = 0, bool zoom = true) { }
    }
}
#endif
