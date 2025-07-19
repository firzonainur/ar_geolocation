using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Mapbox.Utils;
using ARLocation;
using ARLocation.MapboxRoutes;
using Vuplex.WebView;
using Vuplex.WebView.Demos;

namespace ARLocation.MapboxRoutes.SampleProject
{
    [System.Serializable]
    public class ShortcutLocation
    {
        public string Name;
        public double Latitude;
        public double Longitude;
        public string Link;
    }


    public class MenuController : MonoBehaviour
    {
        public enum LineType { Route, NextTarget }

        [Header("Mapbox Settings")]
        public string MapboxToken = "pk.eyJ1IjoiZG1iZm0iLCJhIjoiY2tyYW9hdGMwNGt6dTJ2bzhieDg3NGJxNyJ9.qaQsMUbyu4iARFe0XB2SWg";

        private Vector3 lastCameraPos = Vector3.zero;


        [Header("AR Session Settings")]
        public GameObject ARSession;
        public GameObject ARSessionOrigin;
        public GameObject RouteContainer;
        public Camera Camera;
        public Camera MapboxMapCamera;

        [Header("Route Settings")]
        public MapboxRoute MapboxRoute;
        public AbstractRouteRenderer RoutePathRenderer;
        public AbstractRouteRenderer NextTargetPathRenderer;
        public Mapbox.Unity.Map.AbstractMap Map;
        public Texture RenderTexture;
        [Range(100, 800)] public int MapSize = 400;
        public Material MinimapLineMaterial;
        public int MinimapLayer;
        public float BaseLineWidth = 2;
        public float MinimapStepSize = 0.5f;

        [Header("UI Components")]
        public TMP_InputField searchInput;
        public Button searchButton;
        public TMP_Dropdown quickDestinationsDropdown;
        public Button quickDestinationGoButton;
        public TMP_Dropdown searchResultsDropdown; // <--- tambahan untuk hasil search
        public Slider zoomSlider;

        [Header("Quick Destinations")]
        public List<ShortcutLocation> ShortcutLocations = new List<ShortcutLocation>();

        private ShortcutLocation lastShortcutUsed;


        private State s = new State();
        private RouteResponse currentResponse;
        private GameObject minimapRouteGo;

        [System.Serializable]
        private class State
        {
            public string QueryText = "";
            public List<GeocodingFeature> Results = new List<GeocodingFeature>();
            public View View = View.SearchMenu;
            public Location destination;
            public LineType LineType = LineType.NextTarget;
            public string ErrorMessage;
        }

        enum View { SearchMenu, Route }

        [Header("WebView")]
        public CanvasWebViewPrefab webViewPrefab;
        public GameObject WebView3D;

        private void Start()
        {
            NextTargetPathRenderer.enabled = false;
            RoutePathRenderer.enabled = false;
            ARLocationProvider.Instance.OnEnabled.AddListener(OnLocationEnabled);
            Map.OnUpdated += OnMapRedrawn;

            searchButton.onClick.AddListener(OnSearchButtonClicked);
            quickDestinationGoButton.onClick.AddListener(OnQuickDestinationGoClicked);
            searchResultsDropdown.onValueChanged.AddListener(OnSearchResultSelected); // <-- listener untuk pilih hasil search

            zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);

            LoadQuickDestinations();
        }

        private void LoadQuickDestinations()
        {
            quickDestinationsDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            foreach (var shortcut in ShortcutLocations)
            {
                options.Add(new TMP_Dropdown.OptionData(shortcut.Name));
            }

            quickDestinationsDropdown.AddOptions(options);
        }

        private void OnSearchButtonClicked()
        {
            s.QueryText = searchInput.text;
            StartCoroutine(Search(s.QueryText));
        }

        private void OnQuickDestinationGoClicked()
        {
            int index = quickDestinationsDropdown.value;
            if (index >= 0 && index < ShortcutLocations.Count)
            {
                StartShortcutRoute(ShortcutLocations[index]);
            }
        }

        private void OnSearchResultSelected(int index)
        {
            if (index >= 0 && index < s.Results.Count)
            {
                StartRoute(s.Results[index].geometry.coordinates[0]);
            }
        }

        private void OnZoomSliderChanged(float value)
        {
            if (Map != null)
            {
                Map.SetZoom(value);
                Map.UpdateMap();
            }
        }

        public void StartShortcutRoute(ShortcutLocation shortcut)
        {
            if (!string.IsNullOrEmpty(shortcut.Link))
            {
                //ShowWebView(shortcut.Link);
                // Jangan return di sini, lanjut ke navigasi
            }

            var destination = new Location
            {
                Latitude = shortcut.Latitude,
                Longitude = shortcut.Longitude,
                Altitude = 0
            };

            lastShortcutUsed = shortcut;

            StartRoute(destination);
        }

        private async void ShowWebView(string url)
        {
            if (webViewPrefab == null)
            {
                Debug.LogError("WebViewPrefab is not assigned!");
                return;
            }

            webViewPrefab.gameObject.SetActive(true); // Tampilkan canvas WebView

            await webViewPrefab.WaitUntilInitialized();

            PlayerPrefs.SetString("WebUrl3D", url);
            PlayerPrefs.Save();

            webViewPrefab.WebView.LoadUrl(url);
        }

        public void StartRoute(Location dest)
        {
            s.destination = dest;

            if (ARLocationProvider.Instance.IsEnabled)
            {
                LoadRoute(ARLocationProvider.Instance.CurrentLocation.ToLocation());
            }
            else
            {
                ARLocationProvider.Instance.OnEnabled.AddListener(LoadRoute);
            }
        }

        private void LoadRoute(Location _)
        {
            if (s.destination != null)
            {
                var api = new MapboxApi(MapboxToken);
                var loader = new RouteLoader(api);
                StartCoroutine(loader.LoadRoute(
                    new RouteWaypoint { Type = RouteWaypointType.UserLocation },
                    new RouteWaypoint { Type = RouteWaypointType.Location, Location = s.destination },
                    (err, res) =>
                    {
                        if (err != null)
                        {
                            s.ErrorMessage = err;
                            s.Results = new List<GeocodingFeature>();
                            Debug.LogError(err);
                            return;
                        }

                        ARSession.SetActive(true);
                        ARSessionOrigin.SetActive(true);
                        RouteContainer.SetActive(true);
                        Camera.gameObject.SetActive(false);
                        s.View = View.Route;

                        currentPathRenderer.enabled = true;
                        MapboxRoute.RoutePathRenderer = currentPathRenderer;
                        MapboxRoute.BuildRoute(res);

                        currentResponse = res;
                        BuildMinimapRoute(res);
                    }));
            }
        }

        IEnumerator Search(string query)
        {
            var api = new MapboxApi(MapboxToken);
            yield return api.QueryLocal(query, true);

            if (api.ErrorMessage != null)
            {
                s.ErrorMessage = api.ErrorMessage;
                s.Results = new List<GeocodingFeature>();
            }
            else
            {
                s.Results = api.QueryLocalResult.features;
                UpdateSearchResultsDropdown();
            }
        }

        private void UpdateSearchResultsDropdown()
        {
            searchResultsDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            foreach (var result in s.Results)
            {
                options.Add(new TMP_Dropdown.OptionData(result.place_name));
            }

            searchResultsDropdown.AddOptions(options);
        }

        private AbstractRouteRenderer currentPathRenderer => s.LineType == LineType.Route ? RoutePathRenderer : NextTargetPathRenderer;

        private void BuildMinimapRoute(RouteResponse res)
        {
            var geo = res.routes[0].geometry;
            var worldPositions = new List<Vector2>();

            foreach (var p in geo.coordinates)
            {
                var pos = Map.GeoToWorldPosition(new Vector2d(p.Latitude, p.Longitude), true);
                worldPositions.Add(new Vector2(pos.x, pos.z));
            }

            if (minimapRouteGo != null)
            {
                Destroy(minimapRouteGo);
            }

            minimapRouteGo = new GameObject("MinimapRoute");
            minimapRouteGo.layer = MinimapLayer;
            var mesh = minimapRouteGo.AddComponent<MeshFilter>().mesh;
            var lineWidth = BaseLineWidth * Mathf.Pow(2.0f, Map.Zoom - 18);
            LineBuilder.BuildLineMesh(worldPositions, mesh, lineWidth);

            var meshRenderer = minimapRouteGo.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = MinimapLineMaterial;
        }

        private void OnLocationEnabled(Location location)
        {
            Map.SetCenterLatitudeLongitude(new Vector2d(location.Latitude, location.Longitude));
            Map.UpdateMap();
        }

        private void OnMapRedrawn()
        {
            if (currentResponse != null)
            {
                BuildMinimapRoute(currentResponse);
            }
        }

        private void Update()
        {
            if (s.View == View.Route)
            {
                var cameraPos = Camera.main.transform.position;
                var arLocationRootAngle = ARLocationManager.Instance.transform.localEulerAngles.y;
                var cameraAngle = Camera.main.transform.localEulerAngles.y;
                var mapAngle = cameraAngle - arLocationRootAngle;
                MapboxMapCamera.transform.eulerAngles = new Vector3(90, mapAngle, 0);
            }
            else
            {
                MapboxMapCamera.transform.eulerAngles = new Vector3(90, 0, 0);
            }

            string status = PlayerPrefs.GetString("WebViewStatus", "nonaktif");
            if (status == "aktif")
            {
                if (lastShortcutUsed != null && !string.IsNullOrEmpty(lastShortcutUsed.Link))
                {
                    ShowWebView(lastShortcutUsed.Link);
                    WebView3D.SetActive(true);
                }
            }
            else
            {
                WebView3D.SetActive(false);
            }

        }

        private void LateUpdate()
        {
            if (s.View == View.Route)
            {
                UpdateMapFollowCamera();
            }
            else
            {
                MapboxMapCamera.transform.eulerAngles = new Vector3(90, 0, 0);
            }
        }


        public LineType PathRendererType
        {
            get => s.LineType;
            set
            {
                if (value != s.LineType)
                {
                    currentPathRenderer.enabled = false;
                    s.LineType = value;
                    currentPathRenderer.enabled = true;

                    if (s.View == View.Route)
                    {
                        MapboxRoute.RoutePathRenderer = currentPathRenderer;
                    }
                }
            }
        }

        public void EndRoute()
        {
            ARLocationProvider.Instance.OnEnabled.RemoveListener(LoadRoute);
            ARSession.SetActive(false);
            ARSessionOrigin.SetActive(false);
            RouteContainer.SetActive(false);
            Camera.gameObject.SetActive(true);
            s.View = View.SearchMenu;
            if (lastShortcutUsed != null && !string.IsNullOrEmpty(lastShortcutUsed.Link))
            {
                ShowWebView(lastShortcutUsed.Link);
                WebView3D.SetActive(true);
            }
        }

        private void UpdateMapFollowCamera()
        {
            var cameraPos = Camera.main.transform.position;

            var arLocationRootAngle = ARLocationManager.Instance.gameObject.transform.localEulerAngles.y;
            var cameraAngle = Camera.main.transform.localEulerAngles.y;
            var mapAngle = cameraAngle - arLocationRootAngle;

            MapboxMapCamera.transform.eulerAngles = new Vector3(90, mapAngle, 0);

            if ((cameraPos - lastCameraPos).magnitude < MinimapStepSize)
            {
                return;
            }

            lastCameraPos = cameraPos;

            var location = ARLocationManager.Instance.GetLocationForWorldPosition(cameraPos);
            Map.SetCenterLatitudeLongitude(new Vector2d(location.Latitude, location.Longitude));
            Map.UpdateMap();
        }

    }
}
