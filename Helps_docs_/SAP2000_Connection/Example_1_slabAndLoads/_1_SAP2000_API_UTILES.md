PropertyArea
Syntax
SapObject.SapModel.SelectObj.PropertyArea

VB6 Procedure
Function PropertyArea(ByVal Name As String, Optional ByVal DeSelect As Boolean = False) As Long

Parameters
Name

The name of an existing area section property.

DeSelect

The item is False if objects are to be selected and True if they are to be deselected.

Remarks
This function selects or deselects all area objects to which the specified section has been assigned.

The function returns zero if the selection is successfully completed, otherwise it returns nonzero.

VBA Example
Sub SelectAreaProperty()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret as Long

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model from template
      ret = SapModel.File.NewWall(4, 48, 4, 48)

   'select by area property
      ret = SapModel.SelectObj.PropertyArea("ASEC1")

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub




////////////
SetLoadUniform
Syntax
SapObject.SapModel.AreaObj.SetLoadUniform

VB6 Procedure
Function SetLoadUniform(ByVal Name As String, ByVal LoadPat As String, ByVal Value As Double, ByVal Dir As Long, Optional ByVal Replace As Boolean = True, Optional ByVal CSys As String = "Global", Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name

The name of an existing area object or group, depending on the value of the ItemType item.

LoadPat

The name of a defined load pattern.

Value

The uniform load value. [F/L2]

Dir

This is an integer between 1 and 11, indicating the direction of the load.

1 = Local 1 axis (only applies when CSys is Local)

2 = Local 2 axis (only applies when CSys is Local)

3 = Local 3 axis (only applies when CSys is Local)

4 = X direction (does not apply when CSys is Local)

5 = Y direction (does not apply when CSys is Local)

6 = Z direction (does not apply when CSys is Local)

7 = Projected X direction (does not apply when CSys is Local)

8 = Projected Y direction (does not apply when CSys is Local)

9 = Projected Z direction (does not apply when CSys is Local)

10 = Gravity direction (only applies when CSys is Global)

11 = Projected Gravity direction (only applies when CSys is Global)

 

The positive gravity direction (see Dir = 10 and 11) is in the negative Global Z direction.

Replace

If this item is True, all previous uniform loads, if any, assigned to the specified area object(s), in the specified load pattern, are deleted before making the new assignment.

CSys

This is Local or the name of a defined coordinate system, indicating the coordinate system in which the uniform load is specified.

ItemType

This is one of the following items in the eItemType enumeration:

Object = 0

Group = 1

SelectedObjects = 2

 

If this item is Object, the assignment is made to the area object specified by the Name item.

If this item is Group, the assignment is made to all area objects in the group specified by the Name item.

If this item is SelectedObjects, assignment is made to all selected area objects, and the Name item is ignored.

Remarks
This function assigns uniform loads to area objects.



////////////////////////////////////////////////////////////
SetRestraint
Syntax
SapObject.SapModel.PointObj.SetRestraint

VB6 Procedure
Function SetRestraint(ByVal Name As String, ByRef Value() As Boolean, Optional ByVal ItemType As eItemType = object) As Long

Parameters
Name

The name of an existing point object or group depending on the value of the ItemType item.

Value

This is an array of six restraint values.

Value(0) = U1

Value(1) = U2

Value(2) = U3

Value(3) = R1

Value(4) = R2

Value(5) = R3

ItemType

This is one of the following items in the eItemType enumeration:

Object = 0

Group = 1

SelectedObjects = 2

 

If this item is Object, the restraint assignment is made to the point object specified by the Name item.

If this item is Group, the restraint assignment is made to all point objects in the group specified by the Name item.

If this item is SelectedObjects, the restraint assignment is made to all selected point objects and the Name item is ignored.

Remarks
This function assigns the restraint assignments for a point object. The restraint assignments are always set in the point local coordinate system.

The function returns zero if the restraint assignments are successfully assigned, otherwise it returns a nonzero value.

VBA Example
Sub AssignPointRestraints()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret As Long
      Dim i As Long
      Dim Value() As Boolean

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model from template
      ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

   'assign point object restraints
      Redim Value(5)
      For i = 0 to 5
         Value(i) = True
      Next i
      ret = SapModel.PointObj.setRestraint("1", Value)

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub




////////////////////////////////////////////////////////////

SetSpecialPoint
Syntax
SapObject.SapModel.PointObj.SetSpecialPoint

VB6 Procedure
Function SetSpecialPoint(ByVal Name As String, ByVal SpecialPoint As Boolean, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name

The name of an existing point object or group depending on the value of the ItemType item.

SpecialPoint

This item is True if the point object is specified as a special point, otherwise it is False.

ItemType

This is one of the following items in the eItemType enumeration:

Object = 0

Group = 1

SelectedObjects = 2



If this item is Object, the special point status is set for the point object specified by the Name item.

If this item is Group, the special point status is set for all point objects in the group specified by the Name item.

If this item is SelectedObjects, the special point status is set for all selected point objects and the Name item is ignored.

Remarks
This function sets the special point status for a point object.

The function returns zero if the special point status is successfully set, otherwise it returns a nonzero value.

Special points are allowed to exist in the model even if no objects (line, area, solid, link) are connected to them. Points that are not special are automatically deleted if no objects connect to them.

VBA Example
Sub SetSpecialPoint()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'set as special point
ret = SapModel.PointObj.SetSpecialPoint("3", True)

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub


//////////////////////////////////////////////
Group
Syntax
SapObject.SapModel.SelectObj.Group

VB6 Procedure
Function Group(ByVal Name As String, Optional ByVal DeSelect As Boolean = False) As Long

Parameters
Name

The name of an existing group.

DeSelect

The item is False if objects are to be selected and True if they are to be deselected.

Remarks
This function selects or deselects all objects in the specified group.

The function returns zero if the selection is successfully completed, otherwise it returns nonzero.

VBA Example
Sub SelectGroup()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret as Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'select group
ret = SapModel.SelectObj.Group("ALL")

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub



/////////////////////////////////////////////
SetConstraint
Syntax
SapObject.SapModel.PointObj.SetConstraint

VB6 Procedure
Function SetConstraint(ByVal Name As String, ConstraintName As String, Optional ByVal ItemType As eItemType = Object, Optional ByVal Replace As Boolean = True) As Long

Parameters
Name
The name of an existing point object or group depending on the value of the ItemType item.

ConstraintName
The name of an existing joint constraint.

ItemType
This is one of the following items in the eItemType enumeration:
Object = 0
Group = 1
SelectedObjects = 2


If this item is Object, the constraint assignment is made to the point object specified by the Name item.
If this item is Group,  the constraint assignment is made to all point objects in the group specified by the Name item.
If this item is SelectedObjects, the constraint assignment is made to all selected point objects and the Name item is ignored.

Replace

If this item is True, all previous joint constraints, if any, assigned to the specified point object(s) are deleted before making the new assignment.

Remarks
This function makes joint constraint assignments to point objects.

The function returns 0 if the assignment is successfully made, otherwise it returns nonzero.

VBA Example
Sub SetConstraintAssignment()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long
Dim i As Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'define a new constraint
ret = SapModel.ConstraintDef.SetDiaphragm("Diaph1")

'define new constraint assignments
For i = 4 To 16 Step 4
ret = SapModel.PointObj.SetConstraint(Format(i), "Diaph1")
Next i

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub






/////////////////////////////
SetBody
Syntax
SapObject.SapModel.ConstraintDef.SetBody

VB6 Procedure
Function SetBody(ByVal Name As String, ByRef Value() As Boolean, Optional ByVal CSys As String = "Global") As Long

Parameters
Name

The name of an existing constraint.

Value

Value is an array of six booleans that indicate which joint degrees of freedom are included in the constraint. In order, the degrees of freedom addressed in the array are UX, UY, UZ, RX, RY and RZ.

CSys

The name of the coordinate system in which the constraint is defined.

Remarks
This function defines a Body constraint. If the specified name is not used for a constraint, a new constraint is defined using the specified name. If the specified name is already used for another Body constraint, the definition of that constraint is modified. If the specified name is already used for some constraint that is not a Body constraint, an error is returned.

The function returns zero if the constraint data is successfully added or modified, otherwise it returns a nonzero value.

VBA Example
Sub SetBodyConstraint()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim i as long
Dim ret As Long
Dim Value() As Boolean

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'define new constraint
redim Value(5)
for i = 0 To 5
Value(i) = True
Next i
ret = SapModel.ConstraintDef.SetBody("Body1", Value)

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub




////////////////////////////////////////////////////////////////

SetLoadForce
Syntax
SapObject.SapModel.PointObj.SetLoadForce

VB6 Procedure
Function SetLoadForce(ByVal Name As String, ByVal LoadPat As String, ByRef Value() As Double, Optional ByVal Replace As Boolean = False, Optional ByVal CSys As String = "Global", Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name

The name of an existing point object or group depending on the value of the ItemType item.

LoadPat

The name of the load pattern for the point load.

Value

This is an array of six point load values.

Value(0) = F1 [F]

Value(1) = F2 [F]

Value(2) = F3 [F]

Value(3) = M1 [FL]

Value(4) = M2 [FL]

Value(5) = M3 [FL]

Replace

If this item is True, all previous point loads, if any, assigned to the specified point object(s) in the specified load pattern are deleted before making the new assignment.

CSys

The name of the coordinate system for the considered point load. This is Local or the name of a defined coordinate system.

ItemType

This is one of the following items in the eItemType enumeration:

Object = 0

Group = 1

SelectedObjects = 2



If this item is Object, the load assignment is made to the point object specified by the Name item.

If this item is Group, the load assignment is made to all point objects in the group specified by the Name item.

If this item is SelectedObjects, the load assignment is made to all selected point objects and the Name item is ignored.

Remarks
This function makes point load assignments to point objects.

The function returns zero if the load assignments are successfully made, otherwise it returns a nonzero value.

VBA Example
Sub SetPointForceLoad()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long
Dim Value() As Double
Dim LoadPat As String
Dim LCStep As Long
Dim CSys As String

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'add point load
Redim Value(5)
Value(0) = 10
ret = SapModel.PointObj.SetLoadForce("1", "DEAD", Value)

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub


//////////////////////////////////////////////////////////



Merge
Syntax
SapObject.SapModel.EditPoint.Merge

VB6 Procedure
Function Merge(ByVal MergeTol As Double, ByRef NumberPoints As Long, ByRef PointName() As String) As Long

Parameters
MergeTol

Point objects within this distance of one another are merged into one point object. [L]

NumberPoints

The number of the selected point objects that still exist after the merge is complete.

PointName

This is an array of the name of each selected point object that still exists after the merge is complete.

Remarks
This function merges selected point objects that are within a specified distance of one another.

The function returns zero if the merge is successful; otherwise it returns a nonzero value.

VBA Example
Sub MergePointObjects()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long
Dim Name As String
Dim Point1 As String, Point2 As String
Dim NumberPoints As Long
Dim PointName() As String

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 2, 144, 2, 288)

'add frame object by coordinates
ret = SapModel.FrameObj.AddByCoord(-400, 0, 288, -289, 0, 288, Name)

'refresh view
ret = SapModel.View.RefreshView(0, False)

'merge point objects
ret = SapModel.SelectObj.ClearSelection
ret = SapModel.PointObj.SetSelected("3", True)
ret = SapModel.FrameObj.GetPoints(Name, Point1, Point2)
ret = SapModel.PointObj.SetSelected(Point2, True)
ret = SapModel.EditPoint.Merge(2, NumberPoints, PointName)

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub

///////////////////
SetGroup
Syntax
SapObject.SapModel.GroupDef.SetGroup

VB6 Procedure
Function SetGroup(ByVal Name As String, Optional ByVal color As Long = -1, Optional ByVal SpecifiedForSelection As Boolean = True, Optional ByVal SpecifiedForSectionCutDefinition As Boolean = True, Optional ByVal SpecifiedForSteelDesign As Boolean = True, Optional ByVal SpecifiedForConcreteDesign As Boolean = True, Optional ByVal SpecifiedForAluminumDesign As Boolean = True, Optional ByVal SpecifiedForColdFormedDesign As Boolean = True, Optional ByVal SpecifiedForStaticNLActiveStage As Boolean = True, Optional ByVal SpecifiedForBridgeResponseOutput As Boolean = True, Optional ByVal SpecifiedForAutoSeismicOutput As Boolean = False, Optional ByVal SpecifiedForAutoWindOutput As Boolean = False, Optional ByVal SpecifiedForMassAndWeight As Boolean = True) As Long

Parameters
Name

This is the name of a group.  If this is the name of an existing group,  that group is modified, otherwise a new group is added.

color

The display color for the group specified as a Long. If this value is input as �1, the program automatically selects a display color for the group.

SpecifiedForSelection

This item is True if the group is specified to be used for selection; otherwise it is False.

SpecifiedForSectionCutDefinition

This item is True if the group is specified to be used for defining section cuts; otherwise it is False.

SpecifiedForSteelDesign

This item is True if the group is specified to be used for defining steel frame design groups; otherwise it is False.

SpecifiedForConcreteDesign

This item is True if the group is specified to be used for defining concrete frame design groups; otherwise it is False.

SpecifiedForAluminumDesign

This item is True if the group is specified to be used for defining aluminum frame design groups; otherwise it is False.

SpecifiedForColdFormedDesign

This item is True if the group is specified to be used for defining cold formed frame design groups; otherwise it is False.

SpecifiedForStaticNLActiveStage

This item is True if the group is specified to be used for defining stages for nonlinear static analysis; otherwise it is False.

SpecifiedForBridgeResponseOutput

This item is True if the group is specified to be used for reporting bridge response output; otherwise it is False.

SpecifiedForAutoSeismicOutput

This item is True if the group is specified to be used for reporting auto seismic loads; otherwise it is False.

SpecifiedForAutoWindOutput

This item is True if the group is specified to be used for reporting auto wind loads; otherwise it is False.

SpecifiedForMassAndWeight

This item is True if the group is specified to be used for reporting group masses and weight; otherwise it is False.

Remarks
The function returns zero if the group data is successfully set, otherwise it returns a nonzero value.

VBA Example
Sub SetGroupData()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret as Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'define new group
ret = SapModel.GroupDef.SetGroup("Group1")

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub



///////////////////////
AddByPoint
Syntax
SapObject.SapModel.AreaObj.AddByPoint

VB6 Procedure
Function AddByPoint(ByVal NumberPoints as Long, ByRef Point() as String, ByRef Name As String, Optional ByVal PropName As String = "Default", Optional ByVal UserName As String = "") As Long

Parameters
NumberPoints

The number of points in the area abject.

Point
This is an array containing the names of the point objects that define the added area object. The point object names should be ordered to run clockwise or counter clockwise around the area object.

Name
This is the name that the program ultimately assigns for the area object. If no UserName is specified, the program assigns a default name to the area object. If a UserName is specified and that name is not used for another area object, the UserName is assigned to the area object; otherwise a default name is assigned to the area object.

PropName
This is Default, None or the name of a defined area property.
If it is Default, the program assigns a default area property to the area object. If it is None, no area property is assigned to the area object. If it is the name of a defined area property, that property is assigned to the area object.

UserName
This is an optional user specified name for the area object. If a UserName is specified and that name is already used for another area object, the program ignores the UserName.

Remarks
This function adds a new area object whose defining points are specified by name.
The function returns zero if the area object is successfully added; otherwise it returns a nonzero value.

VBA Example
Sub AddAreaObjByPoint()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long
Dim Point() As String
Dim Name As String

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 2, 144, 2, 288)

'add area object by points
Redim Point(3)
Point(0) = "1"
Point(1) = "4"
Point(2) = "5"
Point(3) = "2"
ret = SapModel.AreaObj.AddByPoint(4, Point, Name)

'refresh view
ret = SapModel.View.RefreshView(0, False)

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub

///////////////////////////
SetGroupAssign
Syntax
SapObject.SapModel.PointObj.SetGroupAssign

VB6 Procedure
Function SetGroupAssign(ByVal Name As String, ByVal GroupName As String, Optional ByVal Remove As Boolean = False, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name
The name of an existing point object or group depending on the value of the ItemType item.

GroupName
The name of an existing group to which the assignment is made.

Remove
If this item is False, the specified point objects are added to the group specified by the GroupName item. If it is True, the point objects are removed from the group.

ItemType
This is one of the following items in the eItemType enumeration:

Object = 0
Group = 1

SelectedObjects = 2

If this item is Object, the point object specified by the Name item is added or removed from the group specified by the GroupName item.
If this item is Group, all point objects in the group specified by the Name item are added or removed from the group specified by the GroupName item.
If this item is SelectedObjects, all selected point objects are added or removed from the group specified by the GroupName item and the Name item is ignored.

Remarks
This function adds or removes point objects from a specified group.

The function returns zero if the group assignment is successful, otherwise it returns a nonzero value.


////////////////////////////////


SetGroupAssign
Syntax
SapObject.SapModel.AreaObj.SetGroupAssign

VB6 Procedure
Function SetGroupAssign(ByVal Name As String, ByVal GroupName As String, Optional By Val Remove As Boolean = False, Optional ByVal ItemType As eItemType = Object) As Long
Parameters
Name

The name of an existing area object or group, depending on the value of the ItemType item.

GroupName
The name of an existing group to which the assignment is made.

Remove
If this item is False, the specified area objects are added to the group specified by the GroupName item. If it is True, the area objects are removed from the group.

ItemType
This is one of the following items in the eItemType enumeration:
Object = 0
Group = 1
SelectedObjects = 2

If this item is Object, the area object specified by the Name item is added or removed from the group specified by the GroupName item.
If this item is Group, all area objects in the group specified by the Name item are added or removed from the group specified by the GroupName item.
If this item is SelectedObjects, all selected area objects are added or removed from the group specified by the GroupName item, and the Name item is ignored.

Remarks
This function adds or removes area objects from a specified group.

The function returns zero if the group assignment is successful; otherwise it returns a nonzero value.

VBA Example
Sub AddAreaObjectsToGroup()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.NewWall(2, 48, 2, 48)

'define new group
ret = SapModel.GroupDef.SetGroup("Group1")

'add area objects to group
ret = SapModel.AreaObj.SetGroupAssign("1", "Group1")
ret = SapModel.AreaObj.SetGroupAssign("3", "Group1")

'select objects in group
ret = SapModel.SelectObj.Group("Group1")

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub


///////////////////////////////////////


SetGroupAssign
Syntax
SapObject.SapModel.SolidObj.SetGroupAssign

VB6 Procedure
Function SetGroupAssign(ByVal Name As String, ByVal GroupName As String, Optional By Val Remove As Boolean = False, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name

The name of an existing solid object or group, depending on the value of the ItemType item.

GroupName

The name of an existing group to which the assignment is made.

Remove

If this item is False, the specified solid objects are added to the group specified by the GroupName item. If it is True, the solid objects are removed from the group.

ItemType

This is one of the following items in the eItemType enumeration:

Object = 0

Group = 1

SelectedObjects = 2



If this item is Object, the solid object specified by the Name item is added or removed from the group specified by the GroupName item.

If this item is Group, all solid objects in the group specified by the Name item are added or removed from the group specified by the GroupName item.

If this item is SelectedObjects, all selected solid objects are added or removed from the group specified by the GroupName item, and the Name item is ignored.

Remarks
This function adds or removes solid objects from a specified group.

The function returns zero if the group assignment is successful; otherwise it returns a nonzero value.

VBA Example
Sub AddSolidObjectsToGroup()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.NewSolidBlock(300, 400, 200, , , 2, 2, 2)

'define new group
ret = SapModel.GroupDef.SetGroup("Group1")

'add solid objects to group
ret = SapModel.SolidObj.SetGroupAssign("1", "Group1")
ret = SapModel.SolidObj.SetGroupAssign("2", "Group1")
ret = SapModel.SolidObj.SetGroupAssign("3", "Group1")

'select objects in group
ret = SapModel.SelectObj.Group("Group1")

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub


//////////////////////////////////////////////////
SupportedPoints
Syntax
SapObject.SapModel.SelectObj.SupportedPoints

VB6 Procedure
Function SupportedPoints(ByRef DOF() As Boolean, Optional ByVal CSys As String = "Local", Optional ByVal DeSelect As Boolean = False, Optional ByVal SelectRestraints As Boolean = True, Optional ByVal SelectJointSprings As Boolean = True, Optional ByVal SelectLineSprings As Boolean = True, Optional ByVal SelectAreaSprings As Boolean = True, Optional ByVal SelectSolidSprings As Boolean = True, Optional ByVal SelectOneJointLinks As Boolean = True) As Long

Parameters
DOF

This is an array of six booleans for the six degrees of freedom of a point object.

DOF(0) = U1
DOF(1) = U2
DOF(2) = U3
DOF(3) = R1
DOF(4) = R2
DOF(5) = R3

CSys
The name of the coordinate system in which degrees of freedom (DOF) are specified. This is either Local or the name of a defined coordinate system. Local means the point local coordinate system.
DeSelect :The item is False if objects are to be selected and True if they are to be deselected.
SelectRestraints :If this item is True then points with restraint assignments in one of the specified degrees of freedom are selected or deselected.

SelectJointSprings :If this item is True then points with joint spring assignments in one of the specified degrees of freedom are selected or deselected.
SelectLineSprings : If this item is True, points with a contribution from line spring assignments in one of the specified degrees of freedom are selected or deselected.
SelectAreaSprings :If this item is True, points with a contribution from area spring assignments in one of the specified degrees of freedom are selected or deselected.
SelectSolidSprings :If this item is True, points with a contribution from solid surface spring assignments in one of the specified degrees of freedom are selected or deselected.
SelectOneJointLinks  :If this item is True, points with one joint link assignments in one of the specified degrees of freedom are selected or deselected.

Remarks
This function selects or deselects point objects with support in the specified degrees of freedom.
The function returns zero if the selection is successfully completed, otherwise it returns nonzero.


//////////////////////////////////////


Syntax
SapObject.SapModel.SelectObj.Constraint

VB6 Procedure
Function Constraint(ByVal Name As String, Optional ByVal DeSelect As Boolean = False) As Long

Parameters
Name

The name of an existing joint constraint.

DeSelect

The item is False if objects are to be selected and True if they are to be deselected.

Remarks
This function selects or deselects all point objects to which the specified constraint has been assigned.

The function returns zero if the selection is successfully completed, otherwise it returns nonzero.

VBA Example
Sub SelectConstraint()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret as Long
      Dim i As Long

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model from template
      ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

   'define a new constraint
      ret = SapModel.ConstraintDef.SetDiaphragm("Diaph1")

   'define new constraint assignments
      For i = 4 To 16 Step 4
         ret = SapModel.PointObj.SetConstraint(Format(i), "Diaph1")
      Next i

   'select constraint
      ret = SapModel.SelectObj.Constraint("Diaph1")

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub



////////////////////////////////////////////////

SetSelected
Syntax
SapObject.SapModel.PointObj.SetSelected

VB6 Procedure
Function SetSelected(ByVal Name As String, ByVal Selected As Boolean, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name
The name of an existing point object or group depending on the value of the ItemType item.
Selected
This item is True if the specified point object is selected, otherwise it is False.
ItemType
This is one of the following items in the eItemType enumeration:
Object = 0
Group = 1
SelectedObjects = 2

If this item is Object, the selected status is set for the point object specified by the Name item.
If this item is Group, the selected status is set for all point objects in the group specified by the Name item.
If this item is SelectedObjects, the selected status is set for all selected point objects and the Name item is ignored.

Remarks
This function sets the selected status for a point object.



////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////



////////////////////////////////////////////////////////////

SetSpecialPoint
Syntax
SapObject.SapModel.PointObj.SetSpecialPoint

VB6 Procedure
Function SetSpecialPoint(ByVal Name As String, ByVal SpecialPoint As Boolean, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name

The name of an existing point object or group depending on the value of the ItemType item.

SpecialPoint

This item is True if the point object is specified as a special point, otherwise it is False.

ItemType

This is one of the following items in the eItemType enumeration:

Object = 0

Group = 1

SelectedObjects = 2



If this item is Object, the special point status is set for the point object specified by the Name item.

If this item is Group, the special point status is set for all point objects in the group specified by the Name item.

If this item is SelectedObjects, the special point status is set for all selected point objects and the Name item is ignored.

Remarks
This function sets the special point status for a point object.

The function returns zero if the special point status is successfully set, otherwise it returns a nonzero value.

Special points are allowed to exist in the model even if no objects (line, area, solid, link) are connected to them. Points that are not special are automatically deleted if no objects connect to them.

VBA Example
Sub SetSpecialPoint()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'set as special point
ret = SapModel.PointObj.SetSpecialPoint("3", True)

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub


//////////////////////////////////////////////
Group
Syntax
SapObject.SapModel.SelectObj.Group

VB6 Procedure
Function Group(ByVal Name As String, Optional ByVal DeSelect As Boolean = False) As Long

Parameters
Name:The name of an existing group.
DeSelect:The item is False if objects are to be selected and True if they are to be deselected.

Remarks:This function selects or deselects all objects in the specified group.
The function returns zero if the selection is successfully completed, otherwise it returns nonzero.

VBA Example
Sub SelectGroup()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret as Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'select group
ret = SapModel.SelectObj.Group("ALL")

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub



/////////////////////////////////////////////
SetConstraint
Syntax
SapObject.SapModel.PointObj.SetConstraint

VB6 Procedure
Function SetConstraint(ByVal Name As String, ConstraintName As String, Optional ByVal ItemType As eItemType = Object, Optional ByVal Replace As Boolean = True) As Long

Parameters
Name:The name of an existing point object or group depending on the value of the ItemType item.

ConstraintName:The name of an existing joint constraint.

ItemType:This is one of the following items in the eItemType enumeration:

Object = 0
Group = 1
SelectedObjects = 2

If this item is Object, the constraint assignment is made to the point object specified by the Name item.
If this item is Group,  the constraint assignment is made to all point objects in the group specified by the Name item.
If this item is SelectedObjects, the constraint assignment is made to all selected point objects and the Name item is ignored.

Replace

If this item is True, all previous joint constraints, if any, assigned to the specified point object(s) are deleted before making the new assignment.

Remarks
This function makes joint constraint assignments to point objects.

The function returns 0 if the assignment is successfully made, otherwise it returns nonzero.

VBA Example
Sub SetConstraintAssignment()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long
Dim i As Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'define a new constraint
ret = SapModel.ConstraintDef.SetDiaphragm("Diaph1")

'define new constraint assignments
For i = 4 To 16 Step 4
ret = SapModel.PointObj.SetConstraint(Format(i), "Diaph1")
Next i

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub



//////////////////////////////////////////////////////////


Merge
Syntax
SapObject.SapModel.EditPoint.Merge

VB6 Procedure
Function Merge(ByVal MergeTol As Double, ByRef NumberPoints As Long, ByRef PointName() As String) As Long

Parameters
MergeTol:Point objects within this distance of one another are merged into one point object. [L]

NumberPoints:The number of the selected point objects that still exist after the merge is complete.

PointName:This is an array of the name of each selected point object that still exists after the merge is complete.

Remarks
This function merges selected point objects that are within a specified distance of one another.
The function returns zero if the merge is successful; otherwise it returns a nonzero value.

VBA Example
Sub MergePointObjects()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long
Dim Name As String
Dim Point1 As String, Point2 As String
Dim NumberPoints As Long
Dim PointName() As String

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 2, 144, 2, 288)

'add frame object by coordinates
ret = SapModel.FrameObj.AddByCoord(-400, 0, 288, -289, 0, 288, Name)

'refresh view
ret = SapModel.View.RefreshView(0, False)

'merge point objects
ret = SapModel.SelectObj.ClearSelection
ret = SapModel.PointObj.SetSelected("3", True)
ret = SapModel.FrameObj.GetPoints(Name, Point1, Point2)
ret = SapModel.PointObj.SetSelected(Point2, True)
ret = SapModel.EditPoint.Merge(2, NumberPoints, PointName)

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub

///////////////////
SetGroup
Syntax
SapObject.SapModel.GroupDef.SetGroup

VB6 Procedure
Function SetGroup(ByVal Name As String, Optional ByVal color As Long = -1, Optional ByVal SpecifiedForSelection As Boolean = True, Optional ByVal SpecifiedForSectionCutDefinition As Boolean = True, Optional ByVal SpecifiedForSteelDesign As Boolean = True, Optional ByVal SpecifiedForConcreteDesign As Boolean = True, Optional ByVal SpecifiedForAluminumDesign As Boolean = True, Optional ByVal SpecifiedForColdFormedDesign As Boolean = True, Optional ByVal SpecifiedForStaticNLActiveStage As Boolean = True, Optional ByVal SpecifiedForBridgeResponseOutput As Boolean = True, Optional ByVal SpecifiedForAutoSeismicOutput As Boolean = False, Optional ByVal SpecifiedForAutoWindOutput As Boolean = False, Optional ByVal SpecifiedForMassAndWeight As Boolean = True) As Long

Parameters
Name:This is the name of a group.  If this is the name of an existing group,  that group is modified, otherwise a new group is added.

color:The display color for the group specified as a Long. If this value is input as �1, the program automatically selects a display color for the group.

SpecifiedForSelection:This item is True if the group is specified to be used for selection; otherwise it is False.

SpecifiedForSectionCutDefinition:This item is True if the group is specified to be used for defining section cuts; otherwise it is False.

SpecifiedForSteelDesign:This item is True if the group is specified to be used for defining steel frame design groups; otherwise it is False.

SpecifiedForConcreteDesign:This item is True if the group is specified to be used for defining concrete frame design groups; otherwise it is False.

SpecifiedForAluminumDesign:This item is True if the group is specified to be used for defining aluminum frame design groups; otherwise it is False.

SpecifiedForColdFormedDesign:This item is True if the group is specified to be used for defining cold formed frame design groups; otherwise it is False.

SpecifiedForStaticNLActiveStage:This item is True if the group is specified to be used for defining stages for nonlinear static analysis; otherwise it is False.

SpecifiedForBridgeResponseOutput:This item is True if the group is specified to be used for reporting bridge response output; otherwise it is False.

SpecifiedForAutoSeismicOutput: This item is True if the group is specified to be used for reporting auto seismic loads; otherwise it is False.

SpecifiedForAutoWindOutput:This item is True if the group is specified to be used for reporting auto wind loads; otherwise it is False.

SpecifiedForMassAndWeight:This item is True if the group is specified to be used for reporting group masses and weight; otherwise it is False.

Remarks
The function returns zero if the group data is successfully set, otherwise it returns a nonzero value.

VBA Example
Sub SetGroupData()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret as Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

'define new group
ret = SapModel.GroupDef.SetGroup("Group1")

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub



///////////////////////
AddByPoint
Syntax
SapObject.SapModel.AreaObj.AddByPoint

VB6 Procedure
Function AddByPoint(ByVal NumberPoints as Long, ByRef Point() as String, ByRef Name As String, Optional ByVal PropName As String = "Default", Optional ByVal UserName As String = "") As Long

Parameters
NumberPoints:The number of points in the area abject.
Point:This is an array containing the names of the point objects that define the added area object. The point object names should be ordered to run clockwise or counter clockwise around the area object.
Name:This is the name that the program ultimately assigns for the area object. If no UserName is specified, the program assigns a default name to the area object. If a UserName is specified and that name is not used for another area object, the UserName is assigned to the area object; otherwise a default name is assigned to the area object.
PropName:
This is Default, None or the name of a defined area property.
If it is Default, the program assigns a default area property to the area object. If it is None, no area property is assigned to the area object. If it is the name of a defined area property, that property is assigned to the area object.

UserName:This is an optional user specified name for the area object. If a UserName is specified and that name is already used for another area object, the program ignores the UserName.

Remarks
This function adds a new area object whose defining points are specified by name.
The function returns zero if the area object is successfully added; otherwise it returns a nonzero value.

VBA Example
Sub AddAreaObjByPoint()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long
Dim Point() As String
Dim Name As String

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.New2DFrame(PortalFrame, 2, 144, 2, 288)

'add area object by points
Redim Point(3)
Point(0) = "1"
Point(1) = "4"
Point(2) = "5"
Point(3) = "2"
ret = SapModel.AreaObj.AddByPoint(4, Point, Name)

'refresh view
ret = SapModel.View.RefreshView(0, False)

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub

///////////////////////////
SetGroupAssign
Syntax
SapObject.SapModel.PointObj.SetGroupAssign

VB6 Procedure
Function SetGroupAssign(ByVal Name As String, ByVal GroupName As String, Optional ByVal Remove As Boolean = False, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name:The name of an existing point object or group depending on the value of the ItemType item.
GroupName:The name of an existing group to which the assignment is made.
Remove:If this item is False, the specified point objects are added to the group specified by the GroupName item. If it is True, the point objects are removed from the group.

ItemType
This is one of the following items in the eItemType enumeration:
Object = 0
Group = 1

SelectedObjects = 2

If this item is Object, the point object specified by the Name item is added or removed from the group specified by the GroupName item.
If this item is Group, all point objects in the group specified by the Name item are added or removed from the group specified by the GroupName item.
If this item is SelectedObjects, all selected point objects are added or removed from the group specified by the GroupName item and the Name item is ignored.

Remarks
This function adds or removes point objects from a specified group.

The function returns zero if the group assignment is successful, otherwise it returns a nonzero value.


////////////////////////////////


SetGroupAssign
Syntax
SapObject.SapModel.AreaObj.SetGroupAssign

VB6 Procedure
Function SetGroupAssign(ByVal Name As String, ByVal GroupName As String, Optional By Val Remove As Boolean = False, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name:The name of an existing area object or group, depending on the value of the ItemType item.
GroupName:The name of an existing group to which the assignment is made.
Remove:If this item is False, the specified area objects are added to the group specified by the GroupName item. If it is True, the area objects are removed from the group.
ItemType:This is one of the following items in the eItemType enumeration:
Object = 0
Group = 1
SelectedObjects = 2

If this item is Object, the area object specified by the Name item is added or removed from the group specified by the GroupName item.
If this item is Group, all area objects in the group specified by the Name item are added or removed from the group specified by the GroupName item.
If this item is SelectedObjects, all selected area objects are added or removed from the group specified by the GroupName item, and the Name item is ignored.

Remarks
This function adds or removes area objects from a specified group.
The function returns zero if the group assignment is successful; otherwise it returns a nonzero value.

VBA Example
Sub AddAreaObjectsToGroup()
'dimension variables
Dim SapObject as cOAPI
Dim SapModel As cSapModel
Dim ret As Long

'create Sap2000 object
Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

'start Sap2000 application
SapObject.ApplicationStart

'create SapModel object
Set SapModel = SapObject.SapModel

'initialize model
ret = SapModel.InitializeNewModel

'create model from template
ret = SapModel.File.NewWall(2, 48, 2, 48)

'define new group
ret = SapModel.GroupDef.SetGroup("Group1")

'add area objects to group
ret = SapModel.AreaObj.SetGroupAssign("1", "Group1")
ret = SapModel.AreaObj.SetGroupAssign("3", "Group1")

'select objects in group
ret = SapModel.SelectObj.Group("Group1")

'close Sap2000
SapObject.ApplicationExit False
Set SapModel = Nothing
Set SapObject = Nothing
End Sub


/////////////////////////////////////////////////////////


SetGroupAssign
Syntax
SapObject.SapModel.LinkObj.SetGroupAssign

VB6 Procedure
Function SetGroupAssign(ByVal Name As String, ByVal GroupName As String, Optional By Val Remove As Boolean = False, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name:The name of an existing link object or group, depending on the value of the ItemType item.
GroupName:The name of an existing group to which the assignment is made.
Remove:If this item is False, the specified link objects are added to the group specified by the GroupName item. If it is True, the link objects are removed from the group.
ItemType: This is one of the following items in the eItemType enumeration:
Object = 0
Group = 1
SelectedObjects = 2

If this item is Object, the link object specified by the Name item is added or removed from the group specified by the GroupName item.
If this item is Group, all link objects in the group specified by the Name item are added or removed from the group specified by the GroupName item.
If this item is SelectedObjects, all selected link objects are added or removed from the group specified by the GroupName item, and the Name item is ignored.

Remarks
This function adds or removes link objects from a specified group.
The function returns zero if the group assignment is successful; otherwise it returns a nonzero value.



/////////////////////////////////////////////////////////////


AddByCoord
Syntax
SapObject.SapModel.FrameObj.AddByCoord

VB6 Procedure
Function AddByCoord(ByVal xi As Double, ByVal yi As Double, ByVal zi As Double, ByVal xj As Double, ByVal yj As Double, ByVal zj As Double, ByRef Name As String, Optional ByVal PropName As String = "Default", Optional ByVal UserName As String = "", Optional ByVal CSys As String = "Global") As Long

Parameters
xi, yi, zi

The coordinates of the I-End of the added frame object. The coordinates are in the coordinate system defined by the CSys item.

xj, yj, zj

The coordinates of the J-End of the added frame object. The coordinates are in the coordinate system defined by the CSys item.

Name

This is the name that the program ultimately assigns for the frame object. If no UserName is specified, the program assigns a default name to the frame object. If a UserName is specified and that name is not used for another frame, cable or tendon object, the UserName is assigned to the frame object, otherwise a default name is assigned to the frame object.

PropName

This is Default, None, or the name of a defined frame section property.

If it is Default, the program assigns a default section property to the frame object. If it is None, no section property is assigned to the frame object. If it is the name of a defined frame section property, that property is assigned to the frame object.

UserName

This is an optional user specified name for the frame object. If a UserName is specified and that name is already used for another frame object, the program ignores the UserName.

CSys

The name of the coordinate system in which the frame object end point coordinates are defined.

Remarks
This function adds a new frame object whose end points are at the specified coordinates.

The function returns zero if the frame object is successfully added, otherwise it returns a nonzero value.

VBA Example
Sub AddFrameObjByCoord()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret As Long
      Dim Name As String

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model from template
      ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

   'add frame object by coordinates
      ret = SapModel.FrameObj.AddByCoord(-300, 0, 0, -100, 0, 124, Name)

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub

Release Notes
Initial release in version 11.00.



//////
AddCartesian
Syntax
SapObject.SapModel.PointObj.AddCartesian

VB6 Procedure
Function AddCartesian(ByVal x As Double, ByVal y As Double, ByVal z As Double, ByRef Name As String, Optional ByVal userName As String = "", Optional ByVal csys As String = "Global", Optional ByVal MergeOff As Boolean = False, Optional ByVal MergeNumber As Long = 0) As Long

Parameters
x

The X-coordinate of the added point object in the specified coordinate system. [L]

y

The Y-coordinate of the added point object in the specified coordinate system. [L]

z

The Z-coordinate of the added point object in the specified coordinate system. [L]

Name

This is the name that the program ultimately assigns for the point object. If no UserName is specified, the program assigns a default name to the point object. If a UserName is specified and that name is not used for another point, the UserName is assigned to the point; otherwise a default name is assigned to the point.

If a point is merged with another point, this will be the name of the point object with which it was merged.

UserName

This is an optional user specified name for the point object. If a UserName is specified and that name is already used for another point object, the program ignores the UserName.

CSys

The name of the coordinate system in which the joint coordinates are defined.

MergeOff

If this item is False, a new point object that is added at the same location as an existing point object will be merged with the existing point object (assuming the two point objects have the same MergeNumber) and thus only one point object will exist at the location.

If this item is True, the points will not merge and two point objects will exist at the same location.

MergeNumber

Two points objects in the same location will merge only if their merge number assignments are the same. By default all pointobjects have a merge number of zero.

Remarks
This function adds a point object to a model. The added point object will be tagged as a Special Point except if it was merged with another point object. Special points are allowed to exist in the model with no objects connected to them.

The function returns zero if the point object is successfully added or merged, otherwise it returns a nonzero value.

VBA Example
Sub AddPointCartesian()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim x As Double, y As Double, z As Double
      Dim Name as String
      Dim MyName as String
      Dim ret As Long

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model blank from template
      ret = SapModel.File.NewBlank

   'add point object to model
      x = 12
      y = 37
      z = 0
      MyName = "A1"
      ret = SapModel.PointObj.AddCartesian(x, y, z, Name, MyName)


   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub

 

////////////////////
AddByPoint
Syntax
SapObject.SapModel.FrameObj.AddByPoint

VB6 Procedure
Function AddByPoint(ByVal Point1 as String, ByVal Point2 as String, ByRef Name As String, Optional ByVal PropName As String = "Default", Optional ByVal UserName As String = "") As Long

Parameters
Point1

The name of a defined point object at the I-End of the added frame object.

Point2

The name of a defined point object at the J-End of the added frame object.

Name

This is the name that the program ultimately assigns for the frame object. If no UserName is specified, the program assigns a default name to the frame object. If a UserName is specified and that name is not used for another frame, cable or tendon object, the UserName is assigned to the frame object, otherwise a default name is assigned to the frame object.

PropName

This is Default, None, or the name of a defined frame section property.

If it is Default, the program assigns a default section property to the frame object. If it is None, no section property is assigned to the frame object. If it is the name of a defined frame section property, that property is assigned to the frame object.

UserName

This is an optional user specified name for the frame object. If a UserName is specified and that name is already used for another frame object, the program ignores the UserName.

Remarks
This function adds a new frame object whose end points are specified by name.

The function returns zero if the frame object is successfully added, otherwise it returns a nonzero value.

VBA Example
Sub AddFrameObjByPoint()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret As Long
      Dim Name As String

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model from template
      ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

   'add frame object by points
      ret = SapModel.FrameObj.AddByPoint("1", "6", Name)

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing


///////////////////////////////////////////////////
SetLoadForce
Syntax
SapObject.SapModel.PointObj.SetLoadForce

VB6 Procedure
Function SetLoadForce(ByVal Name As String, ByVal LoadPat As String, ByRef Value() As Double, Optional ByVal Replace As Boolean = False, Optional ByVal CSys As String = "Global", Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name   #The name of an existing point object or group depending on the value of the ItemType item.

LoadPat  #The name of the load pattern for the point load.

Value
This is an array of six point load values.
Value(0) = F1 [F]
Value(1) = F2 [F]
Value(2) = F3 [F]
Value(3) = M1 [FL]
Value(4) = M2 [FL]
Value(5) = M3 [FL]

Replace
If this item is True, all previous point loads, if any, assigned to the specified point object(s) in the specified load pattern are deleted before making the new assignment.

CSys
The name of the coordinate system for the considered point load. This is Local or the name of a defined coordinate system.

ItemType
This is one of the following items in the eItemType enumeration:
Object = 0  If this item is Object, the load assignment is made to the point object specified by the Name item.
Group = 1   If this item is Group, the load assignment is made to all point objects in the group specified by the Name item.
SelectedObjects = 2 If this item is SelectedObjects, the load assignment is made to all selected point objects and the Name item is ignored.

Remarks
This function makes point load assignments to point objects.
The function returns zero if the load assignments are successfully made, otherwise it returns a nonzero value.

VBA Example
Sub SetPointForceLoad()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret As Long
      Dim Value() As Double
      Dim LoadPat As String
      Dim LCStep As Long
      Dim CSys As String

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model from template
      ret = SapModel.File.New2DFrame(PortalFrame, 3, 124, 3, 200)

   'add point load
      Redim Value(5)
      Value(0) = 10
      ret = SapModel.PointObj.SetLoadForce("1", "DEAD", Value)

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub

///////////////////////////////////////////////////////

SetSection
Syntax
SapObject.SapModel.FrameObj.SetSection

VB6 Procedure
Function SetSection(ByVal name As String, ByVal PropName As String, Optional ByVal ItemType As eItemType = object, Optional ByVal sVarRelStartLoc As Double = 0, Optional ByVal sVarTotalLength As Double = 0) As Long

Parameters
Name: The name of an existing frame object or group, depending on the value of the ItemType item.
PropName: This is None or the name of a frame section property to be assigned to the specified frame object(s).
ItemType:  This is one of the following items in the eItemType enumeration:
Object = 0
Group = 1
SelectedObjects = 2


If this item is Object, the assignment is made to the frame object specified by the Name item.
If this item is Group, the assignment is made to all frame objects in the group specified by the Name item.
If this item is SelectedObjects, assignment is made to all selected frame objects, and the Name item is ignored.

sVarTotalLength : This is the total assumed length of the nonprismatic section. Enter 0 for this item to indicate that the section length is the same as the frame object length.

This item is applicable only when the assigned frame section property is a nonprismatic section.

sVarRelStartLoc: This is the relative distance along the nonprismatic section to the I-End (start) of the frame object. This item is ignored when the sVarTotalLengthitem is 0.

This item is applicable only when the assigned frame section property is a nonprismatic section, and the sVarTotalLengthitem is greater than zero.

Remarks: This function assigns a frame section property to a frame object.

The function returns zero if the frame section property data is successfully assigned, otherwise it returns a nonzero value.

VBA Example
Sub SetFrameSectionProp()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret As Long
      Dim FileName As String

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'open an existing file
      FileName = "C:\SapAPI\Example 1-022.sdb"
      ret = SapModel.File.OpenFile(FileName)

   'unlock model
      ret = SapModel.SetModelIsLocked(False)

   'set frame section property
      ret = SapModel.FrameObj.SetSection("28", "W24X160")

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub

/////////////////////////////////
SetMaterial
Syntax
SapObject.SapModel.PropMaterial.SetMaterial

VB6 Procedure
Function SetMaterial(ByVal Name As String, ByVal MatType As eMatType, Optional ByVal Color As Long = -1, Optional ByVal Notes As String = "", Optional ByVal GUID As String = "") As Long

Parameters
Name : The name of an existing or new material property. If this is an existing property, that property is modified; otherwise, a new property is added.

MatType: 
This is one of the following items in the eMatType enumeration.

eMatType_Steel = 1
eMatType_Concrete = 2
eMatType_NoDesign = 3
eMatType_Aluminum = 4
eMatType_ColdFormed = 5
eMatType_Rebar = 6
eMatType_Tendon = 7


Color The display color assigned to the material. If Color is specified as -1, the program will automatically assign a color.

Notes The notes, if any, assigned to the material.

GUID The GUID (global unique identifier), if any, assigned to the material. If this item is input as Default, the program assigns a GUID to the material.

Remarks This function initializes a material property. If this function is called for an existing material property, all items for the material are reset to their default value.

The function returns zero if the material is successfully initialized; otherwise it returns a nonzero value.

VBA Example
Sub InitializeMatProp()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret As Long

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model from template
      ret = SapModel.File.New2DFrame(PortalFrame, 2, 144, 2, 288)

   'initialize new material property
      ret = SapModel.PropMaterial.SetMaterial("Steel", eMatType_Steel)

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub

Release Notes
Updated the documentation of the eMatType enumeration in v22.1.0

Initial release in version 11.02.

/////////////////////////
SetAngle_1
Syntax
SapObject.SapModel.PropFrame.SetAngle_1

VB6 Procedure
Function SetAngle_1(ByVal Name As String, ByVal MatProp As String, ByVal t3 As Double, ByVal t2 As Double, ByVal tf As Double, ByVal tw As Double, ByVal FilletRadius As Double, Optional ByVal Color As Long = -1, Optional ByVal Notes As String = "", Optional ByVal GUID As String = "") As Long 

Parameters
Name: The name of an existing or new frame section property. If this is an existing property, that property is modified; otherwise, a new property is added.

MatProp: The name of the material property for the section.
t3: The vertical leg depth. [L]
t2: The horizontal leg width. [L]
tf: The horizontal leg thickness. [L]
tw: The vertical leg thickness. [L]
FilletRadius: The fillet radius. [L]
Color: The display color assigned to the section. If Color is specified as -1, the program will automatically assign a color.
Notes: The notes, if any, assigned to the section.
GUID: The GUID (global unique identifier), if any, assigned to the section. If this item is input as Default, then the program assigns a GUID to the section.
	

/////////////
SetGroupAssign
Syntax
SapObject.SapModel.FrameObj.SetGroupAssign

VB6 Procedure
Function SetGroupAssign(ByVal Name As String, ByVal GroupName As String, Optional By Val Remove As Boolean = False, Optional ByVal ItemType As eItemType = Object) As Long

Parameters
Name

The name of an existing frame object or group, depending on the value of the ItemType item.

GroupName

The name of an existing group to which the assignment is made.

Remove

If this item is False, the specified frame objects are added to the group specified by the GroupName item. If it is True, the frame objects are removed from the group.

ItemType

This is one of the following items in the eItemType enumeration:

Object = 0

Group = 1

SelectedObjects = 2

 

If this item is Object, the frame object specified by the Name item is added or removed from the group specified by the GroupName item.

If this item is Group, all frame objects in the group specified by the Name item are added or removed from the group specified by the GroupName item.

If this item is SelectedObjects, all selected frame objects are added or removed from the group specified by the GroupName item, and the Name item is ignored.

Remarks
This function adds or removes frame objects from a specified group.

The function returns zero if the group assignment is successful, otherwise it returns a nonzero value.

VBA Example
Sub AddFrameObjectsToGroup()
   'dimension variables
      Dim SapObject as cOAPI
      Dim SapModel As cSapModel
      Dim ret As Long

   'create Sap2000 object
      Set SapObject = CreateObject("CSI.SAP2000.API.SapObject")

   'start Sap2000 application
      SapObject.ApplicationStart

   'create SapModel object
      Set SapModel = SapObject.SapModel

   'initialize model
      ret = SapModel.InitializeNewModel

   'create model from template
      ret = SapModel.File.New2DFrame(PortalFrame, 2, 144, 2, 288)

   'define new group
      ret = SapModel.GroupDef.SetGroup("Group1")

   'add frame objects to group
      ret = SapModel.FrameObj.SetGroupAssign("8", "Group1")
      ret = SapModel.FrameObj.SetGroupAssign("10", "Group1")

   'select objects in group
      ret = SapModel.SelectObj.Group("Group1")

   'close Sap2000
      SapObject.ApplicationExit False
      Set SapModel = Nothing
      Set SapObject = Nothing
End Sub

Release Notes

