
// URL to Cesium code
window.CESIUM_BASE_URL = "/Cesium/Current/Build/Cesium/";

// Import cesium code
import * as Cesium from "/Cesium/Current/Build/Cesium/index.js";

// The main function, creating a cesium viewer and populating it with data
export async function main(cesiumContainer) 
{
    // Define  "Home"
    Cesium.Camera.DEFAULT_VIEW_FACTOR = 0;
    Cesium.Camera.DEFAULT_VIEW_RECTANGLE = Cesium.Rectangle.fromDegrees(5, 55, 20, 75);

    // For geojson maps
    Cesium.GeoJsonDataSource.clampToGround = true;

    // Define terrain
    let terrainProvider = await Cesium.CesiumTerrainProvider.fromUrl (
	'https://waapi.webatlas.no/wms-hoyde/tms/cesium_qmesh?api_key=DB124B20-9D21-4647-B65A-16C651553E48',
	{
	    requestVertexNormals: true,
	    credit: 'Norkart AS, NASA (SRTM), Statens Kartverk (Nasjonal Detaljert Høydemodell) og kommunene'
	}
    );

    // Define satelite/ortophoto
    let baseImageryProvider = await Cesium.TileMapServiceImageryProvider.fromUrl (
	'https://waapi.webatlas.no/wms-orto/tms/ortofoto_pri_up_200_8_3857_256?api_key=DB124B20-9D21-4647-B65A-16C651553E48',
	{
	    fileExtension: 'jpg',
	    tilingScheme: new Cesium.WebMercatorTilingScheme({}),
	    maximumLevel: 20,
	    credit: 'Norkart AS, Sentinel-2 cloudless – https://s2maps.eu by EOX IT Services GmbH (Contains modified Copernicus Sentinel data 2016 & 2017), Geovekst og kommunene'
	}
    );

    // Create the viewer
    var cesiumViewer = new Cesium.Viewer(cesiumContainer, {
	// Slight reduction in render resource usage
	requestRenderMode : true,
	// Remove unneccessary data layer menu
	baseLayerPicker: false,
	// Remove animation speed widget
	animation: false,
	// Remove timeline widget
	timeline: false,
	// Remove geocoder widget
	geocoder: false,
	// Set to 3D view, and remove 2D/3D selector
	scene3DOnly : true,
	sceneModePicker: false,
	// enable 3D object animations
	shouldAnimate : true,
	// Add terrain and imagery
	baseLayer  : Cesium.ImageryLayer.fromProviderAsync(baseImageryProvider),
	terrainProvider : terrainProvider,
	// Enable webgl 2.0 and multisampled rendering (looks much better!)
	contextOptions: {
	    requestWebgl2: true,
	},
	msaaSamples : 4
    });
    
    // Increase Terrain tile cache
    cesiumViewer.scene.globe.tileCacheSize = 1000;

    // 3D tiles for buildings
    let building_tileset = await Cesium.Cesium3DTileset.fromUrl(
	"https://waapi.webatlas.no/3d-tiles/tileserver.fcgi/tileset.json?api_key=DB124B20-9D21-4647-B65A-16C651553E48",
	{
	    dynamicScreenSpaceError: true
	}
    );
    // The building style
    building_tileset.style = await Cesium.Cesium3DTileStyle.fromUrl(
	"https://waapi.webatlas.no/3d-tiles/fkb-style.json?api_key=DB124B20-9D21-4647-B65A-16C651553E48");
    // Add to viewer
    cesiumViewer.scene.primitives.add(building_tileset);

    // Trees
    let tree_tileset = await Cesium.Cesium3DTileset.fromUrl(
	"https://waapi.webatlas.no/3d-tiles/tileserver_skog.fcgi/tileset.json?api_key=DB124B20-9D21-4647-B65A-16C651553E48",
	{
	    dynamicScreenSpaceError: true
	}
    );
    cesiumViewer.scene.primitives.add(tree_tileset);
    
    // Make our own Credit display
    cesiumViewer.scene.frameState.creditDisplay.destroy(); 
    cesiumViewer.scene.frameState.creditDisplay = new Cesium.CreditDisplay(cesiumViewer.scene.frameState.creditDisplay.container);
    cesiumViewer.scene.frameState.creditDisplay.addStaticCredit(new Cesium.Credit('<a href="https://www.norkart.no/" target="_blank">' +
										  '<img width=150 src="http://norgei3d.no/logo/norkart_logo_gradient_negativ_n.png" title="Norkart"/></a>', false)); 

    // Remove cesium ion reference
    var cesiumLogoContainer = document.getElementsByClassName("cesium-credit-logoContainer");
    cesiumLogoContainer[0].style.display = "none";

    // Get the URL arguments
    let urlArgs = Cesium.queryToObject(window.location.search.substring(1));

    // Set view from url arguments
    var view = urlArgs.view;
    if (Cesium.defined(view)) {
	var splitQuery = view.split(/[ ,]+/);
	if (splitQuery.length > 1) {
	    var longitude = !isNaN(+splitQuery[0]) ? +splitQuery[0] : 0.0;
	    var latitude = !isNaN(+splitQuery[1]) ? +splitQuery[1] : 0.0;
	    var height =
		splitQuery.length > 2 && !isNaN(+splitQuery[2])
		? +splitQuery[2]
		: 300.0;
	    var heading =
		splitQuery.length > 3 && !isNaN(+splitQuery[3])
		? Cesium.Math.toRadians(+splitQuery[3])
		: undefined;
	    var pitch =
		splitQuery.length > 4 && !isNaN(+splitQuery[4])
		? Cesium.Math.toRadians(+splitQuery[4])
		: undefined;
	    var roll =
		splitQuery.length > 5 && !isNaN(+splitQuery[5])
		? Cesium.Math.toRadians(+splitQuery[5])
		: undefined;

	    cesiumViewer.camera.setView({
		destination: Cesium.Cartesian3.fromDegrees(longitude, latitude, height),
		orientation: {
		    heading: heading,
		    pitch: pitch,
		    roll: roll,
		},
	    });
	}
    }

    // Encode view into url arguments
    var camera = cesiumViewer.camera;
    function saveCamera() {
	var position = camera.positionCartographic;
	var hpr = "";
	if (Cesium.defined(camera.heading)) {
	    hpr =
		"," +
		Cesium.Math.toDegrees(camera.heading) +
		"," +
		Cesium.Math.toDegrees(camera.pitch) +
		"," +
		Cesium.Math.toDegrees(camera.roll);
	}
	urlArgs.view =
	    Cesium.Math.toDegrees(position.longitude) +
	    "," +
	    Cesium.Math.toDegrees(position.latitude) +
	    "," +
	    position.height +
	    hpr;
	history.replaceState(undefined, "", "?" + Cesium.objectToQuery(urlArgs));
    }

    var timeout;
    if (urlArgs.saveCamera !== "false") {
	camera.changed.addEventListener(function () {
	    window.clearTimeout(timeout);
	    timeout = window.setTimeout(saveCamera, 1000);
	});
    }
    
    // Finished
    return cesiumViewer;
}
