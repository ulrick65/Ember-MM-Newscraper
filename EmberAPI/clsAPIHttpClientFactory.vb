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

Imports System.Net
Imports System.Net.Http
Imports System.Threading.Tasks
Imports NLog

''' <summary>
''' Provides a shared, thread-safe HttpClient instance for connection pooling.
''' </summary>
''' <remarks>
''' HttpClient is designed to be instantiated once and reused throughout the application lifetime.
''' Creating new HttpClient instances per request can exhaust available sockets and cause SocketException errors.
''' </remarks>
Public Module HttpClientFactory

#Region "Fields"

    Private ReadOnly _logger As Logger = LogManager.GetCurrentClassLogger()
    Private ReadOnly _syncLock As New Object()
    Private _sharedClient As HttpClient = Nothing
    Private _isInitialized As Boolean = False

#End Region 'Fields

#Region "Properties"

    ''' <summary>
    ''' Gets the shared HttpClient instance. Thread-safe and lazily initialized.
    ''' </summary>
    ''' <returns>A configured HttpClient instance for reuse across the application.</returns>
    Public ReadOnly Property SharedClient As HttpClient
        Get
            EnsureInitialized()
            Return _sharedClient
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    ''' <summary>
    ''' Ensures the shared HttpClient is initialized with proper configuration.
    ''' </summary>
    Private Sub EnsureInitialized()
        If _isInitialized Then Return

        SyncLock _syncLock
            If _isInitialized Then Return

            Try
                Dim handler As New HttpClientHandler()

                ' Configure connection pooling
                handler.MaxConnectionsPerServer = 10

                ' Enable automatic decompression
                handler.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate

                ' Configure proxy if settings exist
                ConfigureProxy(handler)

                ' Create the shared client
                _sharedClient = New HttpClient(handler)

                ' Set default timeout (30 seconds)
                _sharedClient.Timeout = TimeSpan.FromSeconds(30)

                ' Set default headers
                _sharedClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate")

                _isInitialized = True
                _logger.Trace("[HttpClientFactory] Shared HttpClient initialized successfully")

            Catch ex As Exception
                _logger.Error(ex, "[HttpClientFactory] Failed to initialize shared HttpClient")
                ' Fallback to basic client without handler customization
                _sharedClient = New HttpClient()
                _sharedClient.Timeout = TimeSpan.FromSeconds(30)
                _isInitialized = True
            End Try
        End SyncLock
    End Sub

    ''' <summary>
    ''' Configures proxy settings on the HttpClientHandler based on application settings.
    ''' </summary>
    ''' <param name="handler">The HttpClientHandler to configure.</param>
    Private Sub ConfigureProxy(ByVal handler As HttpClientHandler)
        Try
            If Not String.IsNullOrEmpty(Master.eSettings.ProxyURI) AndAlso Master.eSettings.ProxyPort >= 0 Then
                Dim proxy As New WebProxy(Master.eSettings.ProxyURI, Master.eSettings.ProxyPort)
                proxy.BypassProxyOnLocal = True

                If Not String.IsNullOrEmpty(Master.eSettings.ProxyCredentials.UserName) AndAlso
                   Not String.IsNullOrEmpty(Master.eSettings.ProxyCredentials.Password) Then
                    proxy.Credentials = Master.eSettings.ProxyCredentials
                Else
                    proxy.Credentials = CredentialCache.DefaultCredentials
                End If

                handler.Proxy = proxy
                handler.UseProxy = True
                _logger.Trace("[HttpClientFactory] Proxy configured: {0}:{1}", Master.eSettings.ProxyURI, Master.eSettings.ProxyPort)
            End If
        Catch ex As Exception
            _logger.Warn(ex, "[HttpClientFactory] Failed to configure proxy, using default")
        End Try
    End Sub

    ''' <summary>
    ''' Resets the shared HttpClient. Use only when proxy settings change.
    ''' </summary>
    ''' <remarks>
    ''' This should rarely be called. Only use when application proxy settings change
    ''' and a new HttpClient with updated proxy configuration is needed.
    ''' </remarks>
    Public Sub Reset()
        SyncLock _syncLock
            If _sharedClient IsNot Nothing Then
                _sharedClient.Dispose()
                _sharedClient = Nothing
            End If
            _isInitialized = False
            _logger.Trace("[HttpClientFactory] Shared HttpClient reset")
        End SyncLock
    End Sub

#Region "Performance Tracked Methods"

    ''' <summary>
    ''' Downloads string content from URL with performance tracking.
    ''' </summary>
    ''' <param name="url">The URL to download from.</param>
    ''' <param name="operationName">Name for the performance metric (e.g., "HTTP.GetString.TMDB").</param>
    ''' <returns>The downloaded string content.</returns>
    Public Async Function GetStringTrackedAsync(url As String, operationName As String) As Task(Of String)
        Using scope = PerformanceTracker.StartOperation(operationName)
            Return Await SharedClient.GetStringAsync(url).ConfigureAwait(False)
        End Using
    End Function

    ''' <summary>
    ''' Downloads byte array from URL with performance tracking.
    ''' </summary>
    ''' <param name="url">The URL to download from.</param>
    ''' <param name="operationName">Name for the performance metric (e.g., "HTTP.GetBytes.Image").</param>
    ''' <returns>The downloaded byte array.</returns>
    Public Async Function GetByteArrayTrackedAsync(url As String, operationName As String) As Task(Of Byte())
        Using scope = PerformanceTracker.StartOperation(operationName)
            Return Await SharedClient.GetByteArrayAsync(url).ConfigureAwait(False)
        End Using
    End Function

    ''' <summary>
    ''' Downloads stream from URL with performance tracking.
    ''' </summary>
    ''' <param name="url">The URL to download from.</param>
    ''' <param name="operationName">Name for the performance metric (e.g., "HTTP.GetStream.Image").</param>
    ''' <returns>The response stream.</returns>
    Public Async Function GetStreamTrackedAsync(url As String, operationName As String) As Task(Of System.IO.Stream)
        Using scope = PerformanceTracker.StartOperation(operationName)
            Return Await SharedClient.GetStreamAsync(url).ConfigureAwait(False)
        End Using
    End Function

    ''' <summary>
    ''' Sends an HTTP request with performance tracking.
    ''' </summary>
    ''' <param name="request">The HTTP request message.</param>
    ''' <param name="operationName">Name for the performance metric.</param>
    ''' <returns>The HTTP response message.</returns>
    Public Async Function SendTrackedAsync(request As HttpRequestMessage, operationName As String) As Task(Of HttpResponseMessage)
        Using scope = PerformanceTracker.StartOperation(operationName)
            Return Await SharedClient.SendAsync(request).ConfigureAwait(False)
        End Using
    End Function

#End Region 'Performance Tracked Methods

#End Region 'Methods

End Module