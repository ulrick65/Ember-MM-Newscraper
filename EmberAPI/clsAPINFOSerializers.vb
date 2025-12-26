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

Option Strict On
Option Explicit On

Imports System.Xml.Serialization

''' <summary>
''' Provides cached, thread-safe XmlSerializer instances for NFO media types.
''' </summary>
''' <remarks>
''' XmlSerializer construction is expensive as it generates and compiles code at runtime.
''' Caching these instances significantly improves performance for large libraries.
''' Lazy(Of T) ensures thread-safe initialization on first access.
''' XmlSerializer.Serialize() and Deserialize() methods are thread-safe.
''' 
''' Performance Impact:
''' - First access: Normal XmlSerializer construction time
''' - Subsequent accesses: Near-zero overhead (cached instance returned)
''' - Large library scanning: Significant improvement (serializer created once, not per-file)
''' </remarks>
Public NotInheritable Class NFOSerializers

#Region "Fields"

    Private Shared ReadOnly _movieSerializer As New Lazy(Of XmlSerializer)(
        Function() New XmlSerializer(GetType(MediaContainers.Movie)))

    Private Shared ReadOnly _moviesetSerializer As New Lazy(Of XmlSerializer)(
        Function() New XmlSerializer(GetType(MediaContainers.Movieset)))

    Private Shared ReadOnly _tvShowSerializer As New Lazy(Of XmlSerializer)(
        Function() New XmlSerializer(GetType(MediaContainers.TVShow)))

    Private Shared ReadOnly _episodeDetailsSerializer As New Lazy(Of XmlSerializer)(
        Function() New XmlSerializer(GetType(MediaContainers.EpisodeDetails)))

#End Region 'Fields

#Region "Properties"

    ''' <summary>
    ''' Gets the cached XmlSerializer for Movie objects.
    ''' </summary>
    Public Shared ReadOnly Property Movie As XmlSerializer
        Get
            Return _movieSerializer.Value
        End Get
    End Property

    ''' <summary>
    ''' Gets the cached XmlSerializer for Movieset objects.
    ''' </summary>
    Public Shared ReadOnly Property Movieset As XmlSerializer
        Get
            Return _moviesetSerializer.Value
        End Get
    End Property

    ''' <summary>
    ''' Gets the cached XmlSerializer for TVShow objects.
    ''' </summary>
    Public Shared ReadOnly Property TVShow As XmlSerializer
        Get
            Return _tvShowSerializer.Value
        End Get
    End Property

    ''' <summary>
    ''' Gets the cached XmlSerializer for EpisodeDetails objects.
    ''' </summary>
    Public Shared ReadOnly Property EpisodeDetails As XmlSerializer
        Get
            Return _episodeDetailsSerializer.Value
        End Get
    End Property

#End Region 'Properties

End Class