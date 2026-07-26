using Sonic_Tracker.Audio;
using Sonic_Tracker.Audio.Capture;
using Sonic_Tracker.Games;
using Sonic_Tracker.Maps;
using Sonic_Tracker.Memory;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Sonic_Tracker;

public partial class MainWindow : Window
{
    private const double DefaultCameraVisibleMapHeight = 900;
    private const double MaximumCameraZoom = 32.0;
    private const double CameraZoomStep = 1.2;
    private const double BigRingInteractionRadius = 80.0;
    private const double SonicCDObjectInteractionPadding = 24.0;
    private const double AngelIslandAct1TransitionX = 0x00E4;
    private const double AngelIslandAct1TransitionY = 0x02F9;
    private const double AngelIslandAct1StartX = 0x1383;
    private const double AngelIslandAct1StartY = 0x0428;
    private const double AngelIslandAct1SecondHalfOffsetX = 7168;
    private const double AngelIslandAct1SecondHalfOffsetY = 1;
    private const double AngelIslandPhaseDetectionTolerance = 16;
    private const string Sonic3KBigRingResource =
        "Assets/Images/BigRingS3K.png";
    private const string SonicCDGeneratorResource =
        "Assets/Images/GeneratorSCD.png";
    private const string SonicCDRobotCarrierResource =
        "Assets/Images/RobotCarrierSCD.png";

    private readonly DispatcherTimer _positionTimer;

    private readonly SoundFingerprintRecognitionService _audioRecognizer;

    private readonly MatrixTransform _mapCameraTransform = new();

    private readonly Dictionary<BigRingLocation, Image> _bigRingOverlays = [];
    private readonly HashSet<BigRingLocation> _collectedBigRings = [];
    private readonly Dictionary<SonicCDPastObjectLocation, Image>
        _sonicCDObjectOverlays = [];

    private MemoryReader? _memoryReader;
    private Sonic1Reader? _sonic1Reader;
    private SonicCDReader? _sonicCDReader;
    private Sonic2Reader? _sonic2Reader;
    private Sonic3KReader? _sonic3KReader;
    private OriginsGameStateReader? _gameStateReader;
    private OriginsCharacterReader? _characterReader;
    private OriginsSceneIndexReader? _sceneIndexReader;

    private CancellationTokenSource? _audioRecognitionCts;

    private bool _audioRecognizerInitialized;
    private string? _lastRecognizedTrack;
    private string? _recognizedZoneId;
    private GameId _recognizedZoneGame = GameId.Unknown;
    private bool _manualMapOverride;
    private bool _specialStageActive;
    private bool _skyChaseActive;
    private bool _titleScreenActive;
    private int? _audioPauseSceneIndex;
    private string? _audioPauseDisplayName;
    private bool _finalZoneActive;
    private string? _noMapSceneName;
    private bool _originsMenuActive;
    private bool _waitingForZoneDetection;
    private GameId _activeGame = GameId.Unknown;
    private PlayableCharacter? _markerCharacter;
    private SonicPosition? _lastPosition;
    private double _cameraZoom = 1.0;
    private double _currentCameraScale = 1.0;
    private double _cameraCenterX;
    private double _cameraCenterY;
    private bool _cameraFollowingSonic = true;
    private bool _angelIslandAct1SecondHalf;
    private bool _isDraggingMap;
    private Point _dragStartPoint;
    private double _dragStartCenterX;
    private double _dragStartCenterY;

    private readonly LevelStartDetector _levelStartDetector = new();
    private readonly Sonic1ActDetector _sonic1ActDetector = new();

    private ZoneMapDefinition _currentZone =
        ZoneMapRegistry.DefaultZone;

    public MainWindow()
    {
        InitializeComponent();

        MapOverrideComboBox.ItemsSource =
            ZoneMapRegistry.All;

        MapOverrideComboBox.SelectedItem =
            _currentZone;

        LevelCanvas.RenderTransform =
            _mapCameraTransform;

        SetMarkerCharacter(
            PlayableCharacter.Sonic);

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(25)
        };

        _positionTimer.Tick += UpdatePosition;

        _audioRecognizer =
            new SoundFingerprintRecognitionService(
                minimumConfidence: 0.50);

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            LoadLevelMap();
            ShowOriginsMenuState();
        }
        catch (Exception exception)
        {
            ConnectionText.Text =
                "Unable to load the default level map.";

            PositionText.Text =
                exception.Message;

            return;
        }

        ConnectToGame();
    }

    private void ShowOriginsMenuState()
    {
        _activeGame = GameId.SonicOriginsMenu;
        _recognizedZoneId = null;
        _recognizedZoneGame = GameId.Unknown;
        _originsMenuActive = true;
        _waitingForZoneDetection = false;
        _specialStageActive = false;
        _skyChaseActive = false;
        _titleScreenActive = false;
        _finalZoneActive = false;
        _noMapSceneName = null;

        LevelMapImage.Visibility = Visibility.Hidden;
        SonicMarker.Visibility = Visibility.Hidden;

        ShowTrackStateOverlay(
            "SONIC ORIGINS",
            "Waiting for a game to start",
            GameId.SonicOriginsMenu);

        PositionText.Text =
            "SONIC ORIGINS\nMAIN MENU\nMap tracking paused";

        Title =
            "Sonic Tracker - Sonic Origins";
    }

    private void LoadLevelMap()
    {
        LoadLevelMap(_currentZone);
    }

    private void LoadLevelMap(
        ZoneMapDefinition zone)
    {
        if (string.IsNullOrWhiteSpace(
            zone.MapResource))
        {
            throw new InvalidOperationException(
                $"No map resource was specified for {zone.ZoneName}.");
        }

        var mapUri = new Uri(
            $"pack://application:,,,/{zone.MapResource}",
            UriKind.Absolute);

        var mapBitmap = new BitmapImage();

        mapBitmap.BeginInit();
        mapBitmap.UriSource = mapUri;
        mapBitmap.CacheOption =
            BitmapCacheOption.OnLoad;
        mapBitmap.EndInit();
        mapBitmap.Freeze();

        LevelMapImage.Source =
            mapBitmap;

        LevelMapImage.Width =
            mapBitmap.PixelWidth;

        LevelMapImage.Height =
            mapBitmap.PixelHeight;

        LevelCanvas.Width =
            mapBitmap.PixelWidth;

        LevelCanvas.Height =
            mapBitmap.PixelHeight;

        BigRingOverlayCanvas.Width =
            mapBitmap.PixelWidth;

        BigRingOverlayCanvas.Height =
            mapBitmap.PixelHeight;

        SonicCDObjectOverlayCanvas.Width =
            mapBitmap.PixelWidth;

        SonicCDObjectOverlayCanvas.Height =
            mapBitmap.PixelHeight;

        Canvas.SetLeft(
            LevelMapImage,
            0);

        Canvas.SetTop(
            LevelMapImage,
            0);

        _currentZone = zone;
        _angelIslandAct1SecondHalf = false;

        RenderBigRingOverlays(zone);
        RenderSonicCDObjectOverlays(zone);

        _cameraFollowingSonic = true;

        Title =
            $"Sonic Tracker - {zone.DisplayName}";

        MapOverrideComboBox.SelectedItem =
            zone;

        SonicMarker.Visibility =
            Visibility.Hidden;
    }

    private void RenderBigRingOverlays(
        ZoneMapDefinition zone)
    {
        BigRingOverlayCanvas.Children.Clear();
        _bigRingOverlays.Clear();
        _collectedBigRings.Clear();

        IReadOnlyList<BigRingLocation> locations =
            Sonic3KBigRingRegistry.GetLocations(zone);

        if (locations.Count == 0)
        {
            return;
        }

        var ringUri = new Uri(
            $"pack://application:,,,/{Sonic3KBigRingResource}",
            UriKind.Absolute);

        var ringBitmap = new BitmapImage();

        ringBitmap.BeginInit();
        ringBitmap.UriSource = ringUri;
        ringBitmap.CacheOption =
            BitmapCacheOption.OnLoad;
        ringBitmap.EndInit();
        ringBitmap.Freeze();

        foreach (BigRingLocation location in locations)
        {
            var ringImage = new Image
            {
                Source = ringBitmap,
                Width = ringBitmap.PixelWidth,
                Height = ringBitmap.PixelHeight,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(
                ringImage,
                location.X);

            Canvas.SetTop(
                ringImage,
                location.Y);

            BigRingOverlayCanvas.Children.Add(
                ringImage);

            _bigRingOverlays[location] =
                ringImage;
        }
    }

    private void UpdateBigRingInteractions(
        double mapX,
        double mapY)
    {
        if (_currentZone.Game !=
                GameId.Sonic3AndKnuckles ||
            _bigRingOverlays.Count == 0)
        {
            return;
        }

        double radiusSquared =
            BigRingInteractionRadius *
            BigRingInteractionRadius;

        foreach (KeyValuePair<BigRingLocation, Image> overlay
                 in _bigRingOverlays.ToArray())
        {
            BigRingLocation location =
                overlay.Key;

            double ringCenterX =
                location.X +
                overlay.Value.Width / 2.0;

            double ringCenterY =
                location.Y +
                overlay.Value.Height / 2.0;

            double deltaX =
                mapX - ringCenterX;

            double deltaY =
                mapY - ringCenterY;

            if (deltaX * deltaX +
                deltaY * deltaY >
                radiusSquared)
            {
                continue;
            }

            BigRingOverlayCanvas.Children.Remove(
                overlay.Value);

            _bigRingOverlays.Remove(
                location);

            _collectedBigRings.Add(
                location);
        }
    }

    private void RenderSonicCDObjectOverlays(
        ZoneMapDefinition zone)
    {
        SonicCDObjectOverlayCanvas.Children.Clear();
        _sonicCDObjectOverlays.Clear();

        IReadOnlyList<SonicCDPastObjectLocation> locations =
            SonicCDPastObjectRegistry.GetLocations(zone);

        if (locations.Count == 0)
        {
            return;
        }

        BitmapImage generatorBitmap =
            LoadImageResource(
                SonicCDGeneratorResource);

        BitmapImage carrierBitmap =
            LoadImageResource(
                SonicCDRobotCarrierResource);

        foreach (SonicCDPastObjectLocation location in locations)
        {
            BitmapImage bitmap =
                location.Type ==
                    SonicCDPastObjectType.MetalSonicGenerator
                        ? generatorBitmap
                        : carrierBitmap;

            var objectImage = new Image
            {
                Source = bitmap,
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(
                objectImage,
                location.X -
                objectImage.Width / 2.0);

            Canvas.SetTop(
                objectImage,
                location.Y -
                objectImage.Height);

            SonicCDObjectOverlayCanvas.Children.Add(
                objectImage);

            _sonicCDObjectOverlays[location] =
                objectImage;
        }
    }

    private void UpdateSonicCDObjectInteractions(
        double mapX,
        double mapY)
    {
        if (_currentZone.Game != GameId.SonicCD ||
            _sonicCDObjectOverlays.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<SonicCDPastObjectLocation, Image> overlay
                 in _sonicCDObjectOverlays.ToArray())
        {
            SonicCDPastObjectLocation location =
                overlay.Key;

            double objectCenterY =
                location.Y -
                overlay.Value.Height / 2.0;

            double horizontalReach =
                overlay.Value.Width / 2.0 +
                SonicCDObjectInteractionPadding;

            double verticalReach =
                overlay.Value.Height / 2.0 +
                SonicCDObjectInteractionPadding;

            if (Math.Abs(mapX - location.X) >
                    horizontalReach ||
                Math.Abs(mapY - objectCenterY) >
                    verticalReach)
            {
                continue;
            }

            SonicCDObjectOverlayCanvas.Children.Remove(
                overlay.Value);

            _sonicCDObjectOverlays.Remove(
                location);
        }
    }

    private static BitmapImage LoadImageResource(
        string resourcePath)
    {
        var resourceUri = new Uri(
            $"pack://application:,,,/{resourcePath}",
            UriKind.Absolute);

        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.UriSource = resourceUri;
        bitmap.CacheOption =
            BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();

        return bitmap;
    }

    private void ConnectToGame()
    {
        try
        {
            Process? process =
                SonicProcessFinder.Find();

            if (process is null)
            {
                ConnectionText.Text =
                    "Sonic Origins is not running.";

                PositionText.Text =
                    "Open Sonic Origins and enter a supported Sonic 2 zone.";

                return;
            }

            _memoryReader =
                new MemoryReader(process);

            _sonic2Reader =
                new Sonic2Reader(_memoryReader);

            _sonic1Reader =
                new Sonic1Reader(_memoryReader);

            _sonicCDReader =
                new SonicCDReader(_memoryReader);

            _sonic3KReader =
                new Sonic3KReader(_memoryReader);

            _gameStateReader =
                new OriginsGameStateReader(_memoryReader);

            _characterReader =
                new OriginsCharacterReader(_memoryReader);

            _sceneIndexReader =
                new OriginsSceneIndexReader(_memoryReader);

            ConnectionText.Text =
                $"Connected to {process.ProcessName} — PID {process.Id}";

            SonicMarker.Visibility =
                Visibility.Hidden;

            _positionTimer.Start();
        }
        catch (Exception exception)
        {
            ConnectionText.Text =
                "Connection failed.";

            PositionText.Text =
                exception.Message;
        }
    }

    private void UpdatePosition(
        object? sender,
        EventArgs e)
    {
        if (_sonic1Reader is null ||
            _sonicCDReader is null ||
            _sonic2Reader is null ||
            _sonic3KReader is null)
        {
            return;
        }

        try
        {
            UpdateActiveGameFromMemory();
            UpdateMapFromSceneIndex();
            UpdateMarkerCharacterFromMemory();

            if (_originsMenuActive)
            {
                SonicMarker.Visibility = Visibility.Hidden;
                PositionText.Text =
                    "SONIC ORIGINS\nMAIN MENU\nMap tracking paused";
                return;
            }

            if (_activeGame != GameId.Unknown &&
                _activeGame != GameId.Sonic1 &&
                _activeGame != GameId.SonicCD &&
                _activeGame != GameId.Sonic2 &&
                _activeGame != GameId.Sonic3AndKnuckles)
            {
                SonicMarker.Visibility = Visibility.Hidden;
                PositionText.Text =
                    $"{GetGameDisplayName(_activeGame)}\n" +
                    "Game detected\n" +
                    "Map support is not configured yet";
                return;
            }

            if (_waitingForZoneDetection)
            {
                SonicMarker.Visibility = Visibility.Hidden;
                PositionText.Text =
                    $"{GetGameDisplayName(_activeGame)}\n" +
                    "Waiting for zone recognition\n" +
                    "Start live recognition to select a map";
                return;
            }

            SonicPosition position =
                _activeGame switch
                {
                    GameId.Sonic1 =>
                        _sonic1Reader.GetPosition(),
                    GameId.SonicCD =>
                        _sonicCDReader.GetPosition(),
                    GameId.Sonic3AndKnuckles =>
                        _sonic3KReader.GetPosition(),
                    _ =>
                        _sonic2Reader.GetPosition()
                };

            _lastPosition = position;

            if (!_specialStageActive &&
                !_skyChaseActive &&
                !_titleScreenActive &&
                !_finalZoneActive)
            {
                DetectActFromPosition(position);
            }

            UpdateSonicMarker(position);

            PositionText.Text = GetPositionStatusText(position);
        }
        catch (Exception exception)
        {
            _positionTimer.Stop();

            SonicMarker.Visibility =
                Visibility.Hidden;

            ConnectionText.Text =
                "Unable to read Sonic's position.";

            PositionText.Text =
                exception.Message;
        }
    }

    private void UpdateActiveGameFromMemory()
    {
        if (_gameStateReader is null)
        {
            return;
        }

        try
        {
            if (_gameStateReader.TryGetActiveGame(out GameId game))
            {
                SetActiveGame(game);
            }
        }
        catch
        {
            // Ignore transient or unrecognized values and preserve the last state.
        }
    }

    private void UpdateMarkerCharacterFromMemory()
    {
        if (_characterReader is null)
        {
            return;
        }

        try
        {
            if (_characterReader.TryGetLeadCharacter(
                    out PlayableCharacter character))
            {
                SetMarkerCharacter(
                    character);
            }
        }
        catch
        {
            // Preserve the last valid icon during transient pointer reads.
        }
    }

    private void SetMarkerCharacter(
        PlayableCharacter character)
    {
        if (_markerCharacter == character)
        {
            return;
        }

        string fileName =
            character switch
            {
                PlayableCharacter.Tails =>
                    "TailsIcon.png",
                PlayableCharacter.Knuckles =>
                    "KnucklesIcon.png",
                PlayableCharacter.Amy =>
                    "AmyIcon.png",
                _ =>
                    "SonicIcon.png"
            };

        var iconUri = new Uri(
            $"pack://application:,,,/Assets/Images/{fileName}",
            UriKind.Absolute);

        var iconBitmap = new BitmapImage();

        iconBitmap.BeginInit();
        iconBitmap.UriSource = iconUri;
        iconBitmap.CacheOption =
            BitmapCacheOption.OnLoad;
        iconBitmap.EndInit();
        iconBitmap.Freeze();

        SonicMarker.Source =
            iconBitmap;

        _markerCharacter =
            character;
    }

    private void UpdateMapFromSceneIndex()
    {
        if (_sceneIndexReader is null ||
            _manualMapOverride ||
            (_activeGame != GameId.Sonic1 &&
             _activeGame != GameId.SonicCD &&
             _activeGame != GameId.Sonic2 &&
             _activeGame != GameId.Sonic3AndKnuckles) ||
            _originsMenuActive)
        {
            return;
        }

        int sceneIndex;

        try
        {
            sceneIndex =
                _sceneIndexReader.GetSceneIndex();
        }
        catch
        {
            // Coordinate and music detection remain available if this read fails.
            return;
        }

        if (_specialStageActive ||
            _titleScreenActive)
        {
            if (!_audioPauseSceneIndex.HasValue)
            {
                _audioPauseSceneIndex = sceneIndex;
                return;
            }

            if (_audioPauseSceneIndex.Value == sceneIndex)
            {
                return;
            }

            _specialStageActive = false;
            _titleScreenActive = false;
            _audioPauseSceneIndex = null;
            _audioPauseDisplayName = null;
        }

        if (!SceneIndexRegistry.TryFind(
                _activeGame,
                sceneIndex,
                out SceneIndexDefinition? scene) ||
            scene is null)
        {
            return;
        }

        bool musicConflicts =
            _recognizedZoneGame == _activeGame &&
            !string.IsNullOrWhiteSpace(_recognizedZoneId) &&
            !string.Equals(
                _recognizedZoneId,
                scene.ZoneId,
                StringComparison.OrdinalIgnoreCase);

        if (musicConflicts)
        {
            return;
        }

        _waitingForZoneDetection = false;

        if (string.Equals(
                scene.ZoneId,
                "SkyChase",
                StringComparison.OrdinalIgnoreCase))
        {
            ShowSkyChaseState();
            return;
        }

        if (!ZoneMapRegistry.TryFind(
                _activeGame,
                scene.ZoneId,
                scene.ActNumber,
                scene.Variant,
                out ZoneMapDefinition? sceneMap) ||
            sceneMap is null)
        {
            ShowNoMapSceneState(
                scene.DisplayName);
            return;
        }

        bool trackingWasPaused =
            _skyChaseActive ||
            _finalZoneActive;

        _skyChaseActive = false;
        _finalZoneActive = false;
        _noMapSceneName = null;
        LevelMapImage.Visibility = Visibility.Visible;
        HideTrackStateOverlay();

        if (_activeGame == GameId.Sonic1)
        {
            _sonic1ActDetector.SetZone(scene.ZoneId);
        }
        else if (trackingWasPaused)
        {
            _levelStartDetector.Arm();
        }

        SwitchZoneMap(sceneMap);
    }

    private void ShowSkyChaseState()
    {
        if (_skyChaseActive)
        {
            return;
        }

        _specialStageActive = false;
        _skyChaseActive = true;
        _titleScreenActive = false;
        _finalZoneActive = false;

        LevelMapImage.Visibility = Visibility.Hidden;
        SonicMarker.Visibility = Visibility.Hidden;

        ShowTrackStateOverlay(
            "SKY CHASE ZONE",
            "No map is available for this zone");
    }

    private void ShowFinalZoneState()
    {
        ShowNoMapSceneState(
            "Final Zone");
    }

    private void ShowNoMapSceneState(
        string sceneName)
    {
        if (_finalZoneActive &&
            string.Equals(
                _noMapSceneName,
                sceneName,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _specialStageActive = false;
        _skyChaseActive = false;
        _titleScreenActive = false;
        _finalZoneActive = true;
        _noMapSceneName = sceneName;

        LevelMapImage.Visibility = Visibility.Hidden;
        SonicMarker.Visibility = Visibility.Hidden;

        ShowTrackStateOverlay(
            sceneName.ToUpperInvariant(),
            "No map image is available for this scene");
    }

    private void SetActiveGame(
        GameId game)
    {
        if (game == GameId.SonicOriginsMenu)
        {
            if (!_originsMenuActive)
            {
                ShowOriginsMenuState();
            }

            return;
        }

        if (_activeGame == game &&
            !_originsMenuActive)
        {
            return;
        }

        _activeGame = game;
        _originsMenuActive = false;

        if (game == GameId.Sonic1 ||
            game == GameId.SonicCD ||
            game == GameId.Sonic2 ||
            game == GameId.Sonic3AndKnuckles)
        {
            _waitingForZoneDetection = false;
            _specialStageActive = false;
            _skyChaseActive = false;
            _titleScreenActive = false;
            _finalZoneActive = false;
            _noMapSceneName = null;

            LevelMapImage.Visibility = Visibility.Visible;
            HideTrackStateOverlay();

            if (game == GameId.Sonic2)
            {
                _levelStartDetector.Arm();
            }

            return;
        }

        _waitingForZoneDetection = false;

        LevelMapImage.Visibility = Visibility.Hidden;
        SonicMarker.Visibility = Visibility.Hidden;

        ShowTrackStateOverlay(
            GetGameDisplayName(game).ToUpperInvariant(),
            "Game detected - map support is not configured yet");
    }

    private static string GetGameDisplayName(
        GameId game) =>
        game switch
        {
            GameId.Sonic1 => "Sonic the Hedgehog",
            GameId.SonicCD => "Sonic CD",
            GameId.Sonic2 => "Sonic the Hedgehog 2",
            GameId.Sonic3AndKnuckles => "Sonic 3 & Knuckles",
            GameId.SonicOriginsMenu => "Sonic Origins",
            _ => "Unknown game"
        };

    private string GetPositionStatusText(
        SonicPosition position)
    {
        if (_specialStageActive)
        {
            return
                $"{_audioPauseDisplayName ?? "SPECIAL STAGE"}\n" +
                "Map tracking paused\n" +
                $"X:     {position.X:F4}\n" +
                $"Y:     {position.Y:F4}";
        }

        if (_skyChaseActive)
        {
            return
                "SKY CHASE ZONE\n" +
                "No map available\n" +
                "Position tracking paused";
        }

        if (_titleScreenActive)
        {
            return
                $"{_audioPauseDisplayName ?? "TITLE SCREEN"}\n" +
                "No level map displayed\n" +
                "Position tracking paused";
        }

        if (_finalZoneActive)
        {
            return
                $"{_noMapSceneName ?? "NO MAP"}\n" +
                "No map available\n" +
                "Position tracking paused";
        }

        return
            $"{GetCurrentMapDisplayName()}\n" +
            $"Game:  {_currentZone.GameName}\n" +
            $"X:     {position.X:F4}\n" +
            $"Y:     {position.Y:F4}\n" +
            $"Raw X: {position.RawX}\n" +
            $"Raw Y: {position.RawY}";
    }

    private void UpdateSonicMarker(
        SonicPosition position)
    {
        if (_specialStageActive ||
            _skyChaseActive ||
            _titleScreenActive ||
            _finalZoneActive)
        {
            SonicMarker.Visibility =
                Visibility.Hidden;

            return;
        }

        Point mapPosition =
            GetMapPosition(position);

        double mapX =
            mapPosition.X;

        double mapY =
            mapPosition.Y;

        UpdateBigRingInteractions(
            mapX,
            mapY);

        UpdateSonicCDObjectInteractions(
            mapX,
            mapY);

        double markerLeft =
            mapX -
            SonicMarker.Width / 2.0;

        double markerTop =
            mapY -
            SonicMarker.Height / 2.0;

        Canvas.SetLeft(
            SonicMarker,
            markerLeft);

        Canvas.SetTop(
            SonicMarker,
            markerTop);

        bool markerIsOnMap =
            mapX >= 0 &&
            mapX < LevelCanvas.Width &&
            mapY >= 0 &&
            mapY < LevelCanvas.Height;

        SonicMarker.Visibility =
            markerIsOnMap
                ? Visibility.Visible
                : Visibility.Hidden;

        if (markerIsOnMap)
        {
            UpdateMapCamera(position);
        }
    }

    private void UpdateMapCamera(
        SonicPosition position)
    {
        Point mapPosition =
            GetMapPosition(position);

        double mapX =
            mapPosition.X;

        double mapY =
            mapPosition.Y;

        double viewportWidth =
            MapViewport.ActualWidth;

        double viewportHeight =
            MapViewport.ActualHeight;

        if (viewportWidth <= 0 ||
            viewportHeight <= 0 ||
            LevelCanvas.Width <= 0 ||
            LevelCanvas.Height <= 0)
        {
            return;
        }

        _cameraZoom = Math.Clamp(
            _cameraZoom,
            GetMinimumCameraZoom(),
            MaximumCameraZoom);

        double preferredScale =
            viewportHeight /
            DefaultCameraVisibleMapHeight;

        double coverScale = Math.Max(
            viewportWidth / LevelCanvas.Width,
            viewportHeight / LevelCanvas.Height);

        double baseScale = Math.Max(
            preferredScale,
            coverScale);

        double scale =
            baseScale * _cameraZoom;

        _currentCameraScale = scale;

        double inverseScale =
            1.0 / scale;

        CharacterMarkerScaleTransform.ScaleX =
            inverseScale;

        CharacterMarkerScaleTransform.ScaleY =
            inverseScale;

        if (_cameraFollowingSonic)
        {
            _cameraCenterX = mapX;
            _cameraCenterY = mapY;
        }

        double visibleMapWidth =
            viewportWidth / scale;

        double visibleMapHeight =
            viewportHeight / scale;

        double cameraLeft = Math.Clamp(
            _cameraCenterX - visibleMapWidth / 2.0,
            0,
            Math.Max(0, LevelCanvas.Width - visibleMapWidth));

        double cameraTop = Math.Clamp(
            _cameraCenterY - visibleMapHeight / 2.0,
            0,
            Math.Max(0, LevelCanvas.Height - visibleMapHeight));

        _cameraCenterX =
            cameraLeft + visibleMapWidth / 2.0;

        _cameraCenterY =
            cameraTop + visibleMapHeight / 2.0;

        double offsetX =
            LevelCanvas.Width * scale < viewportWidth
                ? (viewportWidth - LevelCanvas.Width * scale) / 2.0
                : -cameraLeft * scale;

        double offsetY =
            LevelCanvas.Height * scale < viewportHeight
                ? (viewportHeight - LevelCanvas.Height * scale) / 2.0
                : -cameraTop * scale;

        _mapCameraTransform.Matrix =
            new Matrix(
                scale,
                0,
                0,
                scale,
                offsetX,
                offsetY);
    }

    private Point GetMapPosition(
        SonicPosition position)
    {
        double offsetX =
            _currentZone.MapOffsetX;

        double offsetY =
            _currentZone.MapOffsetY;

        bool isAngelIslandAct1 =
            _currentZone.Game ==
                GameId.Sonic3AndKnuckles &&
            string.Equals(
                _currentZone.ZoneId,
                "AngelIsland",
                StringComparison.OrdinalIgnoreCase) &&
            _currentZone.ActNumber == 1;

        if (isAngelIslandAct1)
        {
            bool previousSecondHalf =
                _angelIslandAct1SecondHalf;

            bool normalOverride =
                _manualMapOverride &&
                string.Equals(
                    _currentZone.Variant,
                    "Normal",
                    StringComparison.OrdinalIgnoreCase);

            bool burntOverride =
                _manualMapOverride &&
                string.Equals(
                    _currentZone.Variant,
                    "Burnt",
                    StringComparison.OrdinalIgnoreCase);

            bool isAtActStart =
                Math.Abs(
                    position.X -
                    AngelIslandAct1StartX) <=
                    AngelIslandPhaseDetectionTolerance &&
                Math.Abs(
                    position.Y -
                    AngelIslandAct1StartY) <=
                    AngelIslandPhaseDetectionTolerance;

            bool isAtFireTransition =
                Math.Abs(
                    position.X -
                    AngelIslandAct1TransitionX) <=
                    AngelIslandPhaseDetectionTolerance &&
                Math.Abs(
                    position.Y -
                    AngelIslandAct1TransitionY) <=
                    AngelIslandPhaseDetectionTolerance;

            if (normalOverride)
            {
                _angelIslandAct1SecondHalf = false;
            }
            else if (burntOverride)
            {
                _angelIslandAct1SecondHalf = true;
            }
            else if (isAtActStart)
            {
                _angelIslandAct1SecondHalf = false;
            }
            else if (isAtFireTransition)
            {
                _angelIslandAct1SecondHalf = true;
            }

            if (_angelIslandAct1SecondHalf)
            {
                offsetX =
                    AngelIslandAct1SecondHalfOffsetX;

                offsetY =
                    AngelIslandAct1SecondHalfOffsetY;
            }

            if (!_manualMapOverride &&
                previousSecondHalf !=
                    _angelIslandAct1SecondHalf)
            {
                string phase =
                    _angelIslandAct1SecondHalf
                        ? "Burnt"
                        : "Normal";

                AudioStatusText.Text =
                    $"Angel Island Act 1 changed to: {phase}";

                Title =
                    $"Sonic Tracker - {GetCurrentMapDisplayName()}";
            }
        }

        return new Point(
            position.X + offsetX,
            position.Y + offsetY);
    }

    private string GetCurrentMapDisplayName()
    {
        bool isAngelIslandAct1 =
            _currentZone.Game ==
                GameId.Sonic3AndKnuckles &&
            string.Equals(
                _currentZone.ZoneId,
                "AngelIsland",
                StringComparison.OrdinalIgnoreCase) &&
            _currentZone.ActNumber == 1;

        if (!isAngelIslandAct1)
        {
            return _currentZone.DisplayName;
        }

        string phase =
            _angelIslandAct1SecondHalf
                ? "Burnt"
                : "Normal";

        return
            $"Angel Island Zone - Act 1 - {phase}";
    }

    private void MapViewport_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!_specialStageActive &&
            !_skyChaseActive &&
            _lastPosition is SonicPosition position)
        {
            UpdateMapCamera(position);
        }
    }

    private void MapViewport_PreviewMouseWheel(
        object sender,
        System.Windows.Input.MouseWheelEventArgs e)
    {
        ChangeCameraZoom(
            e.Delta > 0
                ? CameraZoomStep
                : 1.0 / CameraZoomStep);

        e.Handled = true;
    }

    private void ZoomInButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ChangeCameraZoom(CameraZoomStep);
    }

    private void ZoomOutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ChangeCameraZoom(1.0 / CameraZoomStep);
    }

    private void RecenterCameraButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _cameraZoom = 1.0;
        _cameraFollowingSonic = true;
        RefreshMapCamera();
    }

    private void ChangeCameraZoom(
        double multiplier)
    {
        double minimumZoom =
            GetMinimumCameraZoom();

        _cameraZoom = Math.Clamp(
            _cameraZoom * multiplier,
            minimumZoom,
            MaximumCameraZoom);

        RefreshMapCamera();
    }

    private void RefreshMapCamera()
    {
        if (!_specialStageActive &&
            !_skyChaseActive &&
            _lastPosition is SonicPosition position)
        {
            UpdateMapCamera(position);
        }
    }

    private double GetMinimumCameraZoom()
    {
        if (MapViewport.ActualWidth <= 0 ||
            MapViewport.ActualHeight <= 0 ||
            LevelCanvas.Width <= 0 ||
            LevelCanvas.Height <= 0)
        {
            return 0.01;
        }

        double fitScale = Math.Min(
            MapViewport.ActualWidth / LevelCanvas.Width,
            MapViewport.ActualHeight / LevelCanvas.Height);

        double preferredScale =
            MapViewport.ActualHeight /
            DefaultCameraVisibleMapHeight;

        double coverScale = Math.Max(
            MapViewport.ActualWidth / LevelCanvas.Width,
            MapViewport.ActualHeight / LevelCanvas.Height);

        double defaultScale = Math.Max(
            preferredScale,
            coverScale);

        return Math.Min(
            1.0,
            fitScale / defaultScale);
    }

    private void MapViewport_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source &&
            FindVisualParent<Button>(source) is not null)
        {
            return;
        }

        _isDraggingMap = true;
        _cameraFollowingSonic = false;
        _dragStartPoint = e.GetPosition(MapViewport);
        _dragStartCenterX = _cameraCenterX;
        _dragStartCenterY = _cameraCenterY;

        MapViewport.CaptureMouse();
        MapViewport.Cursor = Cursors.SizeAll;
        e.Handled = true;
    }

    private void MapViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!_isDraggingMap ||
            _currentCameraScale <= 0)
        {
            return;
        }

        Point currentPoint =
            e.GetPosition(MapViewport);

        _cameraCenterX =
            _dragStartCenterX -
            (currentPoint.X - _dragStartPoint.X) /
            _currentCameraScale;

        _cameraCenterY =
            _dragStartCenterY -
            (currentPoint.Y - _dragStartPoint.Y) /
            _currentCameraScale;

        RefreshMapCamera();
        e.Handled = true;
    }

    private void MapViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        if (!_isDraggingMap)
        {
            return;
        }

        _isDraggingMap = false;
        MapViewport.ReleaseMouseCapture();
        MapViewport.Cursor = Cursors.Arrow;
        e.Handled = true;
    }

    private static T? FindVisualParent<T>(
        DependencyObject child)
        where T : DependencyObject
    {
        DependencyObject? current = child;

        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private void MainWindow_Closing(
        object? sender,
        CancelEventArgs e)
    {
        _audioRecognitionCts?.Cancel();

        _positionTimer.Stop();

        _memoryReader?.Dispose();

        _memoryReader = null;
        _sonic1Reader = null;
        _sonicCDReader = null;
        _sonic2Reader = null;
        _sonic3KReader = null;
        _gameStateReader = null;
        _characterReader = null;
        _sceneIndexReader = null;
    }

    private async Task InitializeAudioRecognizerAsync()
    {
        if (_audioRecognizerInitialized)
        {
            return;
        }

        AudioStatusText.Text =
            "Loading reference tracks...";

        SonicOriginsAudioProfile profile = new();

        await _audioRecognizer.InitializeAsync(
            profile);

        _audioRecognizerInitialized = true;

        AudioStatusText.Text =
            "Reference tracks loaded.";
    }

    private async void NativeAudioTestButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_audioRecognitionCts is not null)
        {
            AudioStatusText.Text =
                "Stopping recognition after the current capture...";

            NativeAudioTestButton.IsEnabled =
                false;

            NativeAudioTestButton.Content =
                "Stopping...";

            _audioRecognitionCts.Cancel();

            return;
        }

        CancellationTokenSource cts =
            new();

        _audioRecognitionCts =
            cts;

        NativeAudioTestButton.Content =
            "Stop Live Recognition";

        try
        {
            await InitializeAudioRecognizerAsync();

            await RunLiveRecognitionLoopAsync(
                cts.Token);
        }
        catch (OperationCanceledException)
        {
            AudioStatusText.Text =
                "Audio recognition stopped.";
        }
        catch (Exception exception)
        {
            AudioStatusText.Text =
                "Live recognition failed.";

            MessageBox.Show(
                "Live audio recognition failed:\n\n" +
                exception.Message,
                "Live Recognition Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            cts.Dispose();

            if (ReferenceEquals(
                _audioRecognitionCts,
                cts))
            {
                _audioRecognitionCts =
                    null;
            }

            NativeAudioTestButton.IsEnabled =
                true;

            NativeAudioTestButton.Content =
                "Start Live Recognition";

        }
    }

    private async Task RunLiveRecognitionLoopAsync(
        CancellationToken cancellationToken)
    {
        using Process? process =
            NativeAudioConnectionTest.FindSonicOrigins();

        if (process is null)
        {
            throw new InvalidOperationException(
                "Sonic Origins was not found. " +
                "Start the game, enter a level, and try again.");
        }

        AudioStatusText.Text =
            $"Listening to Sonic Origins — PID {process.Id}";

        while (!cancellationToken.IsCancellationRequested)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    "Sonic Origins was closed.");
            }

            string? waveFile = null;

            try
            {
                AudioStatusText.Text =
                    "Listening for the current track...";

                waveFile =
                    await NativeAudioConnectionTest.CaptureWavAsync(
                        process.Id,
                        TimeSpan.FromSeconds(6));

                cancellationToken.ThrowIfCancellationRequested();

                AudioStatusText.Text =
                    "Analyzing captured audio...";

                AudioRecognitionResult? result =
                    await _audioRecognizer.RecognizeFileAsync(
                        waveFile);

                cancellationToken.ThrowIfCancellationRequested();

                DisplayRecognitionResult(result);
            }
            finally
            {
                DeleteTemporaryWaveFile(
                    waveFile);
            }

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                cancellationToken);
        }
    }

    private void DisplayRecognitionResult(
        AudioRecognitionResult? result)
    {
        if (result is null)
        {
            AudioStatusText.Text =
                "No confident match found.\n" +
                "Listening again...";

            return;
        }

        string trackName =
            result.Track.DisplayName;

        bool trackChanged =
            !string.Equals(
                trackName,
                _lastRecognizedTrack,
                StringComparison.OrdinalIgnoreCase);

        _lastRecognizedTrack =
            trackName;

        AudioStatusText.Text =
            $"Detected: {trackName}\n" +
            $"Type: {result.Track.Kind}\n" +
            $"Confidence: {result.Confidence:P1}";

        UpdateTrackState(result);

        if (trackChanged)
        {
            OnRecognizedTrackChanged(
                result);
        }
    }

    private void UpdateTrackState(
        AudioRecognitionResult result)
    {
        bool isSpecialStage =
            string.Equals(
                result.Track.ZoneId,
                "SpecialStage",
                StringComparison.OrdinalIgnoreCase);

        bool isSkyChase =
            string.Equals(
                result.Track.ZoneId,
                "SkyChase",
                StringComparison.OrdinalIgnoreCase);

        bool isTitleScreen =
            string.Equals(
                result.Track.ZoneId,
                "TitleScreen",
                StringComparison.OrdinalIgnoreCase);

        bool isLevelSelect =
            string.Equals(
                result.Track.ZoneId,
                "LevelSelect",
                StringComparison.OrdinalIgnoreCase);

        bool isOriginsMainMenu =
            string.Equals(
                result.Track.ZoneId,
                "OriginsMainMenu",
                StringComparison.OrdinalIgnoreCase);

        bool isSonic1FinalZone =
            result.Track.Game == GameId.Sonic1 &&
            string.Equals(
                result.Track.ZoneId,
                "Final",
                StringComparison.OrdinalIgnoreCase);

        if (isOriginsMainMenu)
        {
            ShowOriginsMenuState();

            AudioStatusText.Text =
                "Origins main menu detected - map tracking paused.";

            return;
        }

        if (isSpecialStage)
        {
            bool enteringSpecialStage =
                !_specialStageActive;

            _activeGame = result.Track.Game;
            _originsMenuActive = false;
            _specialStageActive = true;
            _skyChaseActive = false;
            _titleScreenActive = false;
            _audioPauseDisplayName =
                result.Track.DisplayName;
            _finalZoneActive = false;

            LevelMapImage.Visibility =
                Visibility.Hidden;

            SonicMarker.Visibility =
                Visibility.Hidden;

            ShowTrackStateOverlay(
                result.Track.DisplayName.ToUpperInvariant(),
                $"{GetGameDisplayName(result.Track.Game)} - map tracking paused",
                result.Track.Game);

            AudioStatusText.Text =
                $"{result.Track.DisplayName} - map tracking paused.";

            if (enteringSpecialStage)
            {
                CaptureAudioPauseSceneIndex();
            }

            return;
        }

        if (isSkyChase)
        {
            _activeGame = result.Track.Game;
            _originsMenuActive = false;
            _specialStageActive = false;
            _skyChaseActive = true;
            _titleScreenActive = false;
            _finalZoneActive = false;

            LevelMapImage.Visibility =
                Visibility.Hidden;

            SonicMarker.Visibility =
                Visibility.Hidden;

            ShowTrackStateOverlay(
                "SKY CHASE ZONE",
                "No map is available for this zone");

            AudioStatusText.Text =
                "No Sky Chase map is available.";

            return;
        }

        if (isTitleScreen ||
            isLevelSelect)
        {
            bool enteringTitleScreen =
                !_titleScreenActive;

            _activeGame = result.Track.Game;
            _originsMenuActive = false;
            _specialStageActive = false;
            _skyChaseActive = false;
            _titleScreenActive = true;
            _audioPauseDisplayName =
                result.Track.DisplayName;
            _finalZoneActive = false;

            LevelMapImage.Visibility =
                Visibility.Hidden;

            SonicMarker.Visibility =
                Visibility.Hidden;

            ShowTrackStateOverlay(
                result.Track.DisplayName.ToUpperInvariant(),
                "No level map is currently displayed",
                result.Track.Game);

            AudioStatusText.Text =
                $"{result.Track.DisplayName} detected - map tracking paused.";

            if (enteringTitleScreen)
            {
                CaptureAudioPauseSceneIndex();
            }

            return;
        }

        if (isSonic1FinalZone)
        {
            _activeGame = GameId.Sonic1;
            _originsMenuActive = false;
            _specialStageActive = false;
            _skyChaseActive = false;
            _titleScreenActive = false;
            _finalZoneActive = true;

            LevelMapImage.Visibility = Visibility.Hidden;
            SonicMarker.Visibility = Visibility.Hidden;

            ShowTrackStateOverlay(
                "FINAL ZONE",
                "No map is available for this zone");

            return;
        }

        if (result.IsZoneTrack)
        {
            SetActiveGame(result.Track.Game);

            bool trackingWasPaused =
                _specialStageActive ||
                _skyChaseActive ||
                _titleScreenActive ||
                _finalZoneActive;

            _specialStageActive = false;
            _skyChaseActive = false;
            _titleScreenActive = false;
            _finalZoneActive = false;

            LevelMapImage.Visibility =
                Visibility.Visible;

            HideTrackStateOverlay();

            if (trackingWasPaused)
            {
                _levelStartDetector.Arm();

                AudioStatusText.Text =
                    "Zone music restored - map tracking resumed.";

                if (_lastPosition is SonicPosition position)
                {
                    UpdateSonicMarker(position);
                }
            }

            return;
        }

        if (_specialStageActive ||
            _skyChaseActive ||
            _titleScreenActive)
        {
            return;
        }
    }

    private void ShowTrackStateOverlay(
        string title,
        string detail,
        GameId? logoGame = null)
    {
        TrackStateTitleText.Text = title;
        TrackStateDetailText.Text = detail;
        SetTrackStateLogo(logoGame);
        TrackStateOverlay.Visibility = Visibility.Visible;
    }

    private void SetTrackStateLogo(
        GameId? game)
    {
        string? fileName =
            game switch
            {
                GameId.Sonic1 =>
                    "Sonic1Logo.png",
                GameId.SonicCD =>
                    "SonicCDLogo.png",
                GameId.Sonic2 =>
                    "Sonic2Logo.png",
                GameId.Sonic3AndKnuckles =>
                    "Sonic3KLogo.png",
                GameId.SonicOriginsMenu =>
                    "SonicOriginsLogo.png",
                _ =>
                    null
            };

        if (fileName is null)
        {
            TrackStateLogoImage.Source = null;
            TrackStateLogoImage.Visibility =
                Visibility.Collapsed;
            return;
        }

        var logoUri = new Uri(
            $"pack://application:,,,/Assets/Images/{fileName}",
            UriKind.Absolute);

        var logoBitmap = new BitmapImage();

        logoBitmap.BeginInit();
        logoBitmap.UriSource = logoUri;
        logoBitmap.CacheOption =
            BitmapCacheOption.OnLoad;
        logoBitmap.EndInit();
        logoBitmap.Freeze();

        TrackStateLogoImage.Source =
            logoBitmap;
        TrackStateLogoImage.Visibility =
            Visibility.Visible;
    }

    private void CaptureAudioPauseSceneIndex()
    {
        if (_sceneIndexReader is null)
        {
            _audioPauseSceneIndex = null;
            return;
        }

        try
        {
            _audioPauseSceneIndex =
                _sceneIndexReader.GetSceneIndex();
        }
        catch
        {
            _audioPauseSceneIndex = null;
        }
    }

    private void HideTrackStateOverlay()
    {
        TrackStateOverlay.Visibility = Visibility.Collapsed;
    }

    private void OnRecognizedTrackChanged(
        AudioRecognitionResult result)
    {
        if (!result.IsZoneTrack)
        {
            return;
        }

        string trackName =
            result.Track.DisplayName;

        _recognizedZoneId =
            result.Track.ZoneId;

        _recognizedZoneGame =
            result.Track.Game;

        _waitingForZoneDetection = false;

        if (result.Track.Game == GameId.Sonic1)
        {
            _sonic1ActDetector.SetZone(result.Track.ZoneId);
        }
        else
        {
            _levelStartDetector.Arm();
        }

        if (_skyChaseActive ||
            _finalZoneActive)
        {
            return;
        }

        if (_manualMapOverride ||
            string.Equals(
                _currentZone.ZoneId,
                _recognizedZoneId,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        bool zoneFound =
            ZoneMapRegistry.TryFindByTrackName(
                trackName,
                out ZoneMapDefinition? detectedZone);

        if (!zoneFound ||
            detectedZone is null)
        {
            AudioStatusText.Text =
                "No map is registered for this track.";

            return;
        }

        SwitchZoneMap(
            detectedZone);
    }

    private void DetectActFromPosition(
        SonicPosition position)
    {
        string? coordinateZoneId =
            _recognizedZoneGame == _activeGame &&
            !string.IsNullOrWhiteSpace(_recognizedZoneId)
                ? _recognizedZoneId
                : _currentZone.ZoneId;

        if (_activeGame == GameId.Sonic1)
        {
            int currentAct =
                _currentZone.ActNumber ?? 1;

            int? detectedAct =
                _sonic1ActDetector.Observe(
                    position,
                    coordinateZoneId,
                    currentAct,
                    LevelCanvas.Width);

            if (!_manualMapOverride &&
                detectedAct.HasValue &&
                coordinateZoneId is not null &&
                ZoneMapRegistry.TryFind(
                    coordinateZoneId,
                    detectedAct.Value,
                    out ZoneMapDefinition? sonic1Map) &&
                sonic1Map is not null)
            {
                SwitchZoneMap(sonic1Map);
            }

            return;
        }

        LevelStartDefinition? detectedStart =
            _levelStartDetector.Observe(
                position,
                coordinateZoneId);

        if (_manualMapOverride ||
            detectedStart is null)
        {
            return;
        }

        bool mapFound =
            ZoneMapRegistry.TryFind(
                detectedStart.ZoneId,
                detectedStart.ActNumber,
                out ZoneMapDefinition? detectedMap);

        if (!mapFound ||
            detectedMap is null)
        {
            return;
        }

        SwitchZoneMap(detectedMap);
    }

    private void SwitchZoneMap(
        ZoneMapDefinition newZone)
    {
        bool sameMap =
            string.Equals(
                _currentZone.MapResource,
                newZone.MapResource,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                _currentZone.Variant,
                newZone.Variant,
                StringComparison.OrdinalIgnoreCase);

        if (sameMap)
        {
            return;
        }

        try
        {
            LoadLevelMap(
                newZone);

            AudioStatusText.Text =
                $"Map changed to: {newZone.DisplayName}";

            if (_sonic2Reader is not null)
            {
                SonicMarker.Visibility =
                    Visibility.Visible;
            }
        }
        catch (Exception exception)
        {
            AudioStatusText.Text =
                "Unable to load the map.";

            MessageBox.Show(
                $"Unable to load the map for " +
                $"{newZone.ZoneName}:\n\n" +
                exception.Message,
                "Map Loading Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyMapOverrideButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (MapOverrideComboBox.SelectedItem is not ZoneMapDefinition selectedMap)
        {
            return;
        }

        _manualMapOverride = true;
        SwitchZoneMap(selectedMap);

        AudioStatusText.Text =
            $"Manual override: {selectedMap.DisplayName}";
    }

    private void ClearMapOverrideButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _manualMapOverride = false;
        _levelStartDetector.Arm();

        AudioStatusText.Text =
            "Manual override cleared. Automatic switching enabled.";
    }

    private static void DeleteTemporaryWaveFile(
        string? waveFile)
    {
        if (string.IsNullOrWhiteSpace(
            waveFile))
        {
            return;
        }

        try
        {
            if (File.Exists(waveFile))
            {
                File.Delete(waveFile);
            }
        }
        catch
        {
            // Temporary cleanup failure should not stop recognition.
        }
    }
}
