' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Namespace FormUtils

    Public Class Forms

        Public Shared Sub ResizeAndMoveDialog(ByRef dialog As Control, ByRef form As Form)
            'shrink dialog if bigger than the Windows working area
            Dim iWidth As Integer = dialog.Width
            Dim iHeight As Integer = dialog.Height
            If My.Computer.Screen.WorkingArea.Width < iWidth Then
                iWidth = My.Computer.Screen.WorkingArea.Width
            End If
            If My.Computer.Screen.WorkingArea.Height < iHeight Then
                iHeight = My.Computer.Screen.WorkingArea.Height
            End If
            dialog.Size = New Size(iWidth, iHeight)
            'move the dialog to the center of Embers main dialog
            Dim pLeft As Integer
            Dim pTop As Integer
            pLeft = Master.AppPos.Left + (Master.AppPos.Width - dialog.Width) \ 2
            pTop = Master.AppPos.Top + (Master.AppPos.Height - dialog.Height) \ 2
            If pLeft < 0 Then pLeft = 0
            If pTop < 0 Then pTop = 0
            dialog.Location = New Point(pLeft, pTop)
            form.StartPosition = FormStartPosition.Manual
        End Sub

    End Class

    ''' <summary>
    ''' A TextBox control that displays watermark text when empty.
    ''' </summary>
    <DesignerCategory("Code")>
    <ToolboxItem(True)>
    <ToolboxBitmap(GetType(TextBox))>
    Public Class TextBox_with_Watermark
        Inherits TextBox

        Private _waterText As String = "Default Watermark"
        Private _waterColor As Color = Color.Gray
        Private _waterFont As Font
        Private _waterBrush As SolidBrush
        Private _waterContainer As Panel

        Public Sub New()
            MyBase.New()
            ' Only initialize watermark functionality at runtime, not in Designer
            If Not DesignMode AndAlso Not IsInDesignMode() Then
                InitializeWatermark()
            End If
        End Sub

        ''' <summary>
        ''' Additional check for design mode that works during construction
        ''' </summary>
        Private Function IsInDesignMode() As Boolean
            ' LicenseManager check works during constructor when DesignMode property doesn't
            Return LicenseManager.UsageMode = LicenseUsageMode.Designtime
        End Function

        Private Sub InitializeWatermark()
            _waterFont = New Font(Font, FontStyle.Italic)
            _waterBrush = New SolidBrush(_waterColor)

            CreateWatermark()

            AddHandler TextChanged, AddressOf ChangeText
            AddHandler FontChanged, AddressOf ChangeFont
        End Sub

        Private Sub CreateWatermark()
            _waterContainer = New Panel
            Controls.Add(_waterContainer)
            AddHandler _waterContainer.Click, AddressOf Clicked
            AddHandler _waterContainer.Paint, AddressOf Painted
        End Sub

        Private Sub RemoveWatermark()
            If _waterContainer IsNot Nothing Then
                Controls.Remove(_waterContainer)
            End If
        End Sub

        Private Sub ChangeText(sender As Object, e As EventArgs)
            If TextLength <= 0 Then
                CreateWatermark()
            ElseIf TextLength > 0 Then
                RemoveWatermark()
            End If
        End Sub

        Private Sub ChangeFont(sender As Object, e As EventArgs)
            _waterFont = New Font(Font, FontStyle.Italic)
        End Sub

        Private Sub Clicked(sender As Object, e As EventArgs)
            Focus()
        End Sub

        Private Sub Painted(sender As Object, e As PaintEventArgs)
            If _waterContainer Is Nothing Then Exit Sub

            Dim bHasBorder As Boolean = Not BorderStyle = BorderStyle.None
            _waterContainer.Location = New Point(If(bHasBorder, 3, 2), If(bHasBorder, 1, 0))
            _waterContainer.Anchor = AnchorStyles.Left Or AnchorStyles.Right
            _waterContainer.Height = Height - If(bHasBorder, 2, 0)
            _waterContainer.Width = Width - If(bHasBorder, 4, 1)
            _waterBrush = New SolidBrush(_waterColor)

            Dim Graphic As Graphics = e.Graphics
            Graphic.DrawString(_waterText, _waterFont, _waterBrush, New PointF(-2.0!, 1.0!))
        End Sub

        Protected Overrides Sub OnInvalidated(e As System.Windows.Forms.InvalidateEventArgs)
            MyBase.OnInvalidated(e)
            If _waterContainer IsNot Nothing Then
                _waterContainer.Invalidate()
            End If
        End Sub

        <Category("Watermark Attributes")>
        <Description("Sets Watermark Text")>
        <DefaultValue("Default Watermark")>
        Public Property WatermarkText As String
            Get
                Return _waterText
            End Get
            Set(value As String)
                _waterText = value
                Invalidate()
            End Set
        End Property

        <Category("Watermark Attributes")>
        <Description("Sets Watermark Color")>
        Public Property WatermarkColor As Color
            Get
                Return _waterColor
            End Get
            Set(value As Color)
                _waterColor = value
                Invalidate()
            End Set
        End Property
    End Class

End Namespace