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

Imports System.Collections.Concurrent
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports NLog

''' <summary>
''' Thread-safe performance tracking utility for measuring operation timings.
''' Used to establish baselines and validate performance improvements.
''' </summary>
''' <remarks>
''' Reference: EmberMediaManager\docs\PerformanceImprovements-Phase1.md - Item 0
''' </remarks>
Public Class PerformanceTracker

#Region "Fields"

    Private Shared ReadOnly Logger As Logger = LogManager.GetCurrentClassLogger()
    Private Shared ReadOnly Metrics As New ConcurrentDictionary(Of String, MetricData)
    Private Shared _isEnabled As Boolean = True

#End Region 'Fields

#Region "Properties"

    ''' <summary>
    ''' Gets or sets whether performance tracking is enabled.
    ''' When disabled, tracking methods return immediately with minimal overhead.
    ''' </summary>
    Public Shared Property IsEnabled As Boolean
        Get
            Return _isEnabled
        End Get
        Set(value As Boolean)
            _isEnabled = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the number of unique metrics currently being tracked.
    ''' </summary>
    Public Shared ReadOnly Property MetricCount As Integer
        Get
            Return Metrics.Count
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    ''' <summary>
    ''' Starts tracking an operation. Use with Using statement for automatic duration recording.
    ''' </summary>
    ''' <param name="operationName">Unique name for the operation (e.g., "TMDB.GetMovieInfo")</param>
    ''' <returns>Disposable scope that records duration when disposed</returns>
    ''' <example>
    ''' Using scope = PerformanceTracker.StartOperation("TMDB.GetMovieInfo")
    '''     result = GetMovieFromApi(id)
    ''' End Using
    ''' </example>
    Public Shared Function StartOperation(operationName As String) As OperationScope
        Return New OperationScope(operationName, _isEnabled)
    End Function

    ''' <summary>
    ''' Tracks a synchronous operation and returns its result.
    ''' </summary>
    ''' <typeparam name="T">Return type of the operation</typeparam>
    ''' <param name="operationName">Unique name for the operation</param>
    ''' <param name="action">The operation to execute and measure</param>
    ''' <returns>Result of the operation</returns>
    Public Shared Function Track(Of T)(operationName As String, action As Func(Of T)) As T
        If Not _isEnabled Then
            Return action()
        End If

        Dim stopwatch = Diagnostics.Stopwatch.StartNew()
        Try
            Return action()
        Finally
            stopwatch.Stop()
            RecordMetric(operationName, stopwatch.Elapsed.TotalMilliseconds)
        End Try
    End Function

    ''' <summary>
    ''' Tracks a synchronous operation without a return value.
    ''' </summary>
    ''' <param name="operationName">Unique name for the operation</param>
    ''' <param name="action">The operation to execute and measure</param>
    Public Shared Sub Track(operationName As String, action As Action)
        If Not _isEnabled Then
            action()
            Return
        End If

        Dim stopwatch = Diagnostics.Stopwatch.StartNew()
        Try
            action()
        Finally
            stopwatch.Stop()
            RecordMetric(operationName, stopwatch.Elapsed.TotalMilliseconds)
        End Try
    End Sub

    ''' <summary>
    ''' Tracks an asynchronous operation and returns its result.
    ''' </summary>
    ''' <typeparam name="T">Return type of the operation</typeparam>
    ''' <param name="operationName">Unique name for the operation</param>
    ''' <param name="asyncAction">The async operation to execute and measure</param>
    ''' <returns>Task containing the result of the operation</returns>
    Public Shared Async Function TrackAsync(Of T)(operationName As String, asyncAction As Func(Of Task(Of T))) As Task(Of T)
        If Not _isEnabled Then
            Return Await asyncAction()
        End If

        Dim stopwatch = Diagnostics.Stopwatch.StartNew()
        Try
            Return Await asyncAction()
        Finally
            stopwatch.Stop()
            RecordMetric(operationName, stopwatch.Elapsed.TotalMilliseconds)
        End Try
    End Function

    ''' <summary>
    ''' Tracks an asynchronous operation without a return value.
    ''' </summary>
    ''' <param name="operationName">Unique name for the operation</param>
    ''' <param name="asyncAction">The async operation to execute and measure</param>
    Public Shared Async Function TrackAsync(operationName As String, asyncAction As Func(Of Task)) As Task
        If Not _isEnabled Then
            Await asyncAction()
            Return
        End If

        Dim stopwatch = Diagnostics.Stopwatch.StartNew()
        Try
            Await asyncAction()
        Finally
            stopwatch.Stop()
            RecordMetric(operationName, stopwatch.Elapsed.TotalMilliseconds)
        End Try
    End Function

    ''' <summary>
    ''' Records a metric value directly (for external timing scenarios).
    ''' </summary>
    ''' <param name="operationName">Unique name for the operation</param>
    ''' <param name="elapsedMs">Duration in milliseconds</param>
    Public Shared Sub RecordMetric(operationName As String, elapsedMs As Double)
        If Not _isEnabled Then Return

        Metrics.AddOrUpdate(
            operationName,
            Function(key) New MetricData(operationName, elapsedMs),
            Function(key, existing)
                existing.AddSample(elapsedMs)
                Return existing
            End Function)
    End Sub

    ''' <summary>
    ''' Records an arbitrary numeric value for a metric (e.g., counts, sizes).
    ''' This is an alias for RecordMetric to improve code readability when recording
    ''' non-timing values like image counts or byte sizes.
    ''' </summary>
    ''' <param name="metricName">Unique name for the metric (e.g., "SaveAllImages.Movie.ImageCount")</param>
    ''' <param name="value">The numeric value to record</param>
    ''' <remarks>
    ''' Use this method when recording values that are not durations, such as:
    ''' - Image counts: RecordValue("SaveAllImages.Movie.ImageCount", imageCount)
    ''' - Byte sizes: RecordValue("Download.TotalBytes", bytesDownloaded)
    ''' - Item counts: RecordValue("Scrape.ActorCount", actorCount)
    ''' 
    ''' The metric will still track min/max/avg/total statistics for the recorded values.
    ''' </remarks>
    Public Shared Sub RecordValue(metricName As String, value As Double)
        RecordMetric(metricName, value)
    End Sub

    ''' <summary>
    ''' Records an integer value for a metric (convenience overload).
    ''' </summary>
    ''' <param name="metricName">Unique name for the metric</param>
    ''' <param name="value">The integer value to record</param>
    Public Shared Sub RecordValue(metricName As String, value As Integer)
        RecordMetric(metricName, CDbl(value))
    End Sub

    ''' <summary>
    ''' Records a long value for a metric (convenience overload).
    ''' </summary>
    ''' <param name="metricName">Unique name for the metric</param>
    ''' <param name="value">The long value to record</param>
    Public Shared Sub RecordValue(metricName As String, value As Long)
        RecordMetric(metricName, CDbl(value))
    End Sub

    ''' <summary>
    ''' Gets all recorded metrics.
    ''' </summary>
    ''' <returns>Dictionary of metric name to metric data</returns>
    Public Shared Function GetMetrics() As Dictionary(Of String, MetricData)
        Return Metrics.ToDictionary(Function(kvp) kvp.Key, Function(kvp) kvp.Value.Clone())
    End Function

    ''' <summary>
    ''' Gets a specific metric by name.
    ''' </summary>
    ''' <param name="operationName">Name of the operation</param>
    ''' <returns>MetricData if found, Nothing otherwise</returns>
    Public Shared Function GetMetric(operationName As String) As MetricData
        Dim metric As MetricData = Nothing
        If Metrics.TryGetValue(operationName, metric) Then
            Return metric.Clone()
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Logs all metrics to NLog at Info level.
    ''' </summary>
    Public Shared Sub LogAllMetrics()
        If Metrics.IsEmpty Then
            Logger.Info("[PerformanceTracker] No metrics recorded")
            Return
        End If

        Logger.Info("[PerformanceTracker] ===== Performance Metrics Summary =====")
        Logger.Info("[PerformanceTracker] {0,-40} {1,8} {2,10} {3,10} {4,10} {5,12}",
                     "Operation", "Count", "Min(ms)", "Avg(ms)", "Max(ms)", "Total(ms)")
        Logger.Info("[PerformanceTracker] {0}", New String("-"c, 95))

        For Each kvp In Metrics.OrderBy(Function(m) m.Key)
            Dim m = kvp.Value
            Logger.Info("[PerformanceTracker] {0,-40} {1,8} {2,10:F2} {3,10:F2} {4,10:F2} {5,12:F2}",
                         m.Name.Substring(0, Math.Min(40, m.Name.Length)),
                         m.Count,
                         m.MinMs,
                         m.AvgMs,
                         m.MaxMs,
                         m.TotalMs)
        Next

        Logger.Info("[PerformanceTracker] ========================================")
    End Sub

    ''' <summary>
    ''' Exports all metrics to a CSV file for analysis.
    ''' </summary>
    ''' <param name="filePath">Full path to the output CSV file</param>
    Public Shared Sub ExportToCsv(filePath As String)
        Try
            Dim directory = Path.GetDirectoryName(filePath)
            If Not String.IsNullOrEmpty(directory) AndAlso Not IO.Directory.Exists(directory) Then
                IO.Directory.CreateDirectory(directory)
            End If

            Dim sb As New StringBuilder()
            sb.AppendLine("Operation,Count,MinMs,AvgMs,MaxMs,TotalMs,ExportTime")

            Dim exportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            For Each kvp In Metrics.OrderBy(Function(m) m.Key)
                Dim m = kvp.Value
                sb.AppendLine(String.Format("""{0}"",{1},{2:F2},{3:F2},{4:F2},{5:F2},""{6}""",
                                           m.Name.Replace("""", """"""),
                                           m.Count,
                                           m.MinMs,
                                           m.AvgMs,
                                           m.MaxMs,
                                           m.TotalMs,
                                           exportTime))
            Next

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8)
            Logger.Info("[PerformanceTracker] Metrics exported to: {0}", filePath)

        Catch ex As Exception
            Logger.Error(ex, "[PerformanceTracker] Failed to export metrics to CSV")
        End Try
    End Sub

    ''' <summary>
    ''' Clears all recorded metrics.
    ''' </summary>
    Public Shared Sub Reset()
        Metrics.Clear()
        Logger.Debug("[PerformanceTracker] All metrics cleared")
    End Sub

    ''' <summary>
    ''' Gets a formatted summary string of all metrics.
    ''' </summary>
    ''' <returns>Multi-line summary string</returns>
    Public Shared Function GetSummaryString() As String
        If Metrics.IsEmpty Then
            Return "No metrics recorded"
        End If

        Dim sb As New StringBuilder()
        sb.AppendLine("Performance Metrics Summary")
        sb.AppendLine(New String("-"c, 50))

        For Each kvp In Metrics.OrderBy(Function(m) m.Key)
            Dim m = kvp.Value
            sb.AppendLine(String.Format("{0}: Count={1}, Avg={2:F2}ms, Min={3:F2}ms, Max={4:F2}ms",
                                       m.Name, m.Count, m.AvgMs, m.MinMs, m.MaxMs))
        Next

        Return sb.ToString()
    End Function

#End Region 'Methods

End Class

''' <summary>
''' Stores aggregated metric data for a single operation type.
''' Thread-safe for concurrent updates.
''' </summary>
Public Class MetricData

#Region "Fields"

    Private ReadOnly _lock As New Object()
    Private _count As Integer
    Private _totalMs As Double
    Private _minMs As Double
    Private _maxMs As Double

#End Region 'Fields

#Region "Constructors"

    ''' <summary>
    ''' Creates a new MetricData instance with an initial sample.
    ''' </summary>
    ''' <param name="name">Operation name</param>
    ''' <param name="initialMs">First sample duration in milliseconds</param>
    Public Sub New(name As String, initialMs As Double)
        Me.Name = name
        _count = 1
        _totalMs = initialMs
        _minMs = initialMs
        _maxMs = initialMs
    End Sub

    ''' <summary>
    ''' Private constructor for cloning.
    ''' </summary>
    Private Sub New()
    End Sub

#End Region 'Constructors

#Region "Properties"

    ''' <summary>
    ''' Gets the operation name.
    ''' </summary>
    Public Property Name As String

    ''' <summary>
    ''' Gets the number of samples recorded.
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            SyncLock _lock
                Return _count
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets the total duration of all samples in milliseconds.
    ''' </summary>
    Public ReadOnly Property TotalMs As Double
        Get
            SyncLock _lock
                Return _totalMs
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets the minimum duration in milliseconds.
    ''' </summary>
    Public ReadOnly Property MinMs As Double
        Get
            SyncLock _lock
                Return _minMs
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets the maximum duration in milliseconds.
    ''' </summary>
    Public ReadOnly Property MaxMs As Double
        Get
            SyncLock _lock
                Return _maxMs
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets the average duration in milliseconds.
    ''' </summary>
    Public ReadOnly Property AvgMs As Double
        Get
            SyncLock _lock
                If _count = 0 Then Return 0
                Return _totalMs / _count
            End SyncLock
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    ''' <summary>
    ''' Adds a new sample to the metric data.
    ''' </summary>
    ''' <param name="elapsedMs">Duration in milliseconds</param>
    Public Sub AddSample(elapsedMs As Double)
        SyncLock _lock
            _count += 1
            _totalMs += elapsedMs
            If elapsedMs < _minMs Then _minMs = elapsedMs
            If elapsedMs > _maxMs Then _maxMs = elapsedMs
        End SyncLock
    End Sub

    ''' <summary>
    ''' Creates a copy of this metric data.
    ''' </summary>
    ''' <returns>Cloned MetricData instance</returns>
    Public Function Clone() As MetricData
        SyncLock _lock
            Return New MetricData() With {
                .Name = Name,
                ._count = _count,
                ._totalMs = _totalMs,
                ._minMs = _minMs,
                ._maxMs = _maxMs
            }
        End SyncLock
    End Function

#End Region 'Methods

End Class

''' <summary>
''' Disposable scope for automatic operation timing.
''' Records duration when disposed.
''' </summary>
Public Class OperationScope
    Implements IDisposable

#Region "Fields"

    Private ReadOnly _operationName As String
    Private ReadOnly _stopwatch As Stopwatch
    Private ReadOnly _isEnabled As Boolean
    Private _disposed As Boolean

#End Region 'Fields

#Region "Constructors"

    ''' <summary>
    ''' Creates a new operation scope and starts timing.
    ''' </summary>
    ''' <param name="operationName">Name of the operation being tracked</param>
    ''' <param name="isEnabled">Whether tracking is enabled</param>
    Friend Sub New(operationName As String, isEnabled As Boolean)
        _operationName = operationName
        _isEnabled = isEnabled
        If _isEnabled Then
            _stopwatch = Stopwatch.StartNew()
        End If
    End Sub

#End Region 'Constructors

#Region "Properties"

    ''' <summary>
    ''' Gets the elapsed time so far in milliseconds.
    ''' </summary>
    Public ReadOnly Property ElapsedMs As Double
        Get
            If _stopwatch Is Nothing Then Return 0
            Return _stopwatch.Elapsed.TotalMilliseconds
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    ''' <summary>
    ''' Stops timing and records the metric.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True

        If _isEnabled AndAlso _stopwatch IsNot Nothing Then
            _stopwatch.Stop()
            PerformanceTracker.RecordMetric(_operationName, _stopwatch.Elapsed.TotalMilliseconds)
        End If
    End Sub

#End Region 'Methods

End Class