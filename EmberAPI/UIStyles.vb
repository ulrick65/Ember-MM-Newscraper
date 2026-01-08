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

Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Provides centralized UI styling methods for consistent modern appearance across the application.
''' </summary>
''' <remarks>
''' This class implements a flat-style button theme with hover effects, designed to modernize
''' the WinForms UI without requiring external dependencies. All styling is applied at runtime,
''' preserving Designer compatibility.
''' 
''' Usage:
'''   UIStyles.ApplyPrimaryButtonStyle(btnOK)
'''   UIStyles.ApplySecondaryButtonStyle(btnCancel)
'''   UIStyles.ApplyToAllButtons(Me)  ' Apply to all buttons in a form
''' </remarks>
Public Class UIStyles

#Region "Color Definitions"

    ' Primary button colors - TEAL theme
    Private Shared ReadOnly _primaryColor As Color = Color.FromArgb(0, 150, 136)
    Private Shared ReadOnly _primaryHoverColor As Color = Color.FromArgb(0, 130, 118)
    Private Shared ReadOnly _primaryPressedColor As Color = Color.FromArgb(0, 110, 100)

    ' Alternative: PURPLE theme (uncomment to use)
    'Private Shared ReadOnly _primaryColor As Color = Color.FromArgb(111, 66, 193)
    'Private Shared ReadOnly _primaryHoverColor As Color = Color.FromArgb(98, 58, 170)
    'Private Shared ReadOnly _primaryPressedColor As Color = Color.FromArgb(85, 50, 147)

    ' Secondary button colors (Neutral/Gray theme)
    Private Shared ReadOnly _secondaryColor As Color = Color.White
    Private Shared ReadOnly _secondaryHoverColor As Color = Color.FromArgb(240, 240, 240)
    Private Shared ReadOnly _secondaryPressedColor As Color = Color.FromArgb(220, 220, 220)
    Private Shared ReadOnly _secondaryBorderColor As Color = Color.FromArgb(180, 180, 180)
    Private Shared ReadOnly _secondaryTextColor As Color = Color.FromArgb(60, 60, 60)

    ' Disabled state colors
    Private Shared ReadOnly _disabledBackColor As Color = Color.FromArgb(204, 204, 204)
    Private Shared ReadOnly _disabledForeColor As Color = Color.FromArgb(128, 128, 128)

#End Region

#Region "Button Styling Methods"

    ''' <summary>
    ''' Applies modern flat styling with primary (blue) theme to a button.
    ''' </summary>
    ''' <param name="btn">The button to style.</param>
    ''' <remarks>
    ''' Use for primary action buttons like OK, Save, Apply, or important feature buttons.
    ''' </remarks>
    Public Shared Sub ApplyPrimaryButtonStyle(btn As Button)
        If btn Is Nothing Then Return

        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = _primaryColor
        btn.ForeColor = Color.White
        btn.Cursor = Cursors.Hand
        btn.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        btn.FlatAppearance.MouseOverBackColor = _primaryHoverColor
        btn.FlatAppearance.MouseDownBackColor = _primaryPressedColor
    End Sub

    ''' <summary>
    ''' Applies modern flat styling with secondary (neutral/white) theme to a button.
    ''' </summary>
    ''' <param name="btn">The button to style.</param>
    ''' <remarks>
    ''' Use for secondary actions like Cancel, Close, or navigation buttons.
    ''' </remarks>
    Public Shared Sub ApplySecondaryButtonStyle(btn As Button)
        If btn Is Nothing Then Return

        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderColor = _secondaryBorderColor
        btn.FlatAppearance.BorderSize = 1
        btn.BackColor = _secondaryColor
        btn.ForeColor = _secondaryTextColor
        btn.Cursor = Cursors.Hand
        btn.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        btn.FlatAppearance.MouseOverBackColor = _secondaryHoverColor
        btn.FlatAppearance.MouseDownBackColor = _secondaryPressedColor
    End Sub

    ''' <summary>
    ''' Applies styling to all buttons within a control's hierarchy.
    ''' </summary>
    ''' <param name="parent">The parent control containing buttons to style.</param>
    ''' <param name="primaryOnly">If True, applies primary style to all buttons. If False, uses DialogResult to determine style.</param>
    ''' <remarks>
    ''' When primaryOnly is False:
    ''' - Buttons with DialogResult.Cancel get secondary style
    ''' - All other buttons get primary style
    ''' </remarks>
    Public Shared Sub ApplyToAllButtons(parent As Control, Optional primaryOnly As Boolean = False)
        If parent Is Nothing Then Return

        For Each ctrl As Control In parent.Controls
            If TypeOf ctrl Is Button Then
                Dim btn = DirectCast(ctrl, Button)
                If primaryOnly Then
                    ApplyPrimaryButtonStyle(btn)
                Else
                    ' Apply primary to action buttons, secondary to Cancel
                    If btn.DialogResult = DialogResult.Cancel Then
                        ApplySecondaryButtonStyle(btn)
                    Else
                        ApplyPrimaryButtonStyle(btn)
                    End If
                End If
            End If

            ' Recurse into child controls
            If ctrl.HasChildren Then
                ApplyToAllButtons(ctrl, primaryOnly)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Applies rounded corner styling to a button using owner-draw.
    ''' </summary>
    ''' <param name="btn">The button to style.</param>
    ''' <param name="cornerRadius">The radius of the rounded corners (default 8).</param>
    ''' <param name="useGradient">Whether to use a gradient background (default True).</param>
    ''' <remarks>
    ''' This method uses custom painting to achieve rounded corners and optional gradient fills.
    ''' The button must have its Paint event handled for the effect to work.
    ''' </remarks>
    Public Shared Sub ApplyRoundedPrimaryButtonStyle(btn As Button, Optional cornerRadius As Integer = 8, Optional useGradient As Boolean = True)
        If btn Is Nothing Then Return

        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = Color.Transparent
        btn.ForeColor = Color.White
        btn.Cursor = Cursors.Hand
        btn.Font = New Font("Segoe UI", 9, FontStyle.Regular)

        ' Remove any existing paint handlers to avoid duplicates
        RemoveHandler btn.Paint, AddressOf RoundedButton_Paint

        ' Store settings in Tag if not already used, otherwise use a dictionary approach
        btn.Tag = New RoundedButtonSettings With {
            .CornerRadius = cornerRadius,
            .UseGradient = useGradient,
            .NormalColor = _primaryColor,
            .HoverColor = _primaryHoverColor,
            .PressedColor = _primaryPressedColor,
            .IsHovered = False,
            .IsPressed = False
        }

        ' Add event handlers
        AddHandler btn.Paint, AddressOf RoundedButton_Paint
        AddHandler btn.MouseEnter, AddressOf RoundedButton_MouseEnter
        AddHandler btn.MouseLeave, AddressOf RoundedButton_MouseLeave
        AddHandler btn.MouseDown, AddressOf RoundedButton_MouseDown
        AddHandler btn.MouseUp, AddressOf RoundedButton_MouseUp
    End Sub

    Private Shared Sub RoundedButton_Paint(sender As Object, e As PaintEventArgs)
        Dim btn = DirectCast(sender, Button)
        Dim settings = TryCast(btn.Tag, RoundedButtonSettings)
        If settings Is Nothing Then Return

        Dim rect = btn.ClientRectangle
        Dim radius As Integer = settings.CornerRadius

        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        ' Determine current color based on state
        Dim currentColor As Color
        If settings.IsPressed Then
            currentColor = settings.PressedColor
        ElseIf settings.IsHovered Then
            currentColor = settings.HoverColor
        Else
            currentColor = settings.NormalColor
        End If

        ' Create rounded rectangle path
        Using path As New Drawing2D.GraphicsPath()
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90)
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90)
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90)
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90)
            path.CloseFigure()

            ' Fill with gradient or solid color
            If settings.UseGradient Then
                Dim lighterColor = ControlPaint.Light(currentColor, 0.3F)
                Using brush As New Drawing2D.LinearGradientBrush(rect, lighterColor, currentColor, Drawing2D.LinearGradientMode.Vertical)
                    e.Graphics.FillPath(brush, path)
                End Using
            Else
                Using brush As New SolidBrush(currentColor)
                    e.Graphics.FillPath(brush, path)
                End Using
            End If
        End Using

        ' Draw text centered
        TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, rect, Color.White,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
    End Sub

    Private Shared Sub RoundedButton_MouseEnter(sender As Object, e As EventArgs)
        Dim btn = DirectCast(sender, Button)
        Dim settings = TryCast(btn.Tag, RoundedButtonSettings)
        If settings IsNot Nothing Then
            settings.IsHovered = True
            btn.Invalidate()
        End If
    End Sub

    Private Shared Sub RoundedButton_MouseLeave(sender As Object, e As EventArgs)
        Dim btn = DirectCast(sender, Button)
        Dim settings = TryCast(btn.Tag, RoundedButtonSettings)
        If settings IsNot Nothing Then
            settings.IsHovered = False
            settings.IsPressed = False
            btn.Invalidate()
        End If
    End Sub

    Private Shared Sub RoundedButton_MouseDown(sender As Object, e As MouseEventArgs)
        Dim btn = DirectCast(sender, Button)
        Dim settings = TryCast(btn.Tag, RoundedButtonSettings)
        If settings IsNot Nothing Then
            settings.IsPressed = True
            btn.Invalidate()
        End If
    End Sub

    Private Shared Sub RoundedButton_MouseUp(sender As Object, e As MouseEventArgs)
        Dim btn = DirectCast(sender, Button)
        Dim settings = TryCast(btn.Tag, RoundedButtonSettings)
        If settings IsNot Nothing Then
            settings.IsPressed = False
            btn.Invalidate()
        End If
    End Sub

#End Region

#Region "Helper Classes"

    ''' <summary>
    ''' Stores settings for rounded button rendering.
    ''' </summary>
    Private Class RoundedButtonSettings
        Public Property CornerRadius As Integer = 8
        Public Property UseGradient As Boolean = True
        Public Property NormalColor As Color
        Public Property HoverColor As Color
        Public Property PressedColor As Color
        Public Property IsHovered As Boolean = False
        Public Property IsPressed As Boolean = False
    End Class
#End Region

End Class