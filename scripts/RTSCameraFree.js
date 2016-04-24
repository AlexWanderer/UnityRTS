//import System.Collections;

// Speed of scrolling
var scrollSpeed : float;
var scrollEdge : float;
// Whether we want to use the mouse to scroll
var scrollWithMouse;

// Mouse rotation speed
var rotationSpeed : float;

// Zoom scrollwheel speed
var zoomSpeed : float;

// Min/max camera heights
var minCamHeight : float;
var maxCamHeight : float;

function Start() {
    // Init camera height as 70
    Camera.main.transform.position.y = Terrain.activeTerrain.SampleHeight(GetComponent.<Camera>().transform.position)+70;

    scrollSpeed = 90.0;
    scrollEdge = 0.01;
    scrollWithMouse = 0;

    rotationSpeed = 1.0;

    zoomSpeed = 60.0;

    minCamHeight = 15.0;
    maxCamHeight = 300.0;
}

function Update () {
    // We go faster the higher we are
    var y = Camera.main.transform.position.y;
    var right = 0;
    if ( Input.GetKey("d") || (scrollWithMouse && Input.mousePosition.x >= Screen.width * (1 - scrollEdge)) )
        right = 1;
    else if ( Input.GetKey("a") || (scrollWithMouse && Input.mousePosition.x <= Screen.width * scrollEdge) )
        right = -1;

    if ( right ) {
        transform.Translate(
            Time.deltaTime * right * (scrollSpeed + (y - minCamHeight)/2) * System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
            0,
            Time.deltaTime * -right * (scrollSpeed + (y - minCamHeight)/2) *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
            Space.World
        );
    }

    var forward = 0;
    if ( Input.GetKey("w") || (scrollWithMouse && Input.mousePosition.y >= Screen.height * (1 - scrollEdge)) )
        forward = 1;
    else if ( Input.GetKey("s") || (scrollWithMouse && Input.mousePosition.y <= Screen.height * scrollEdge) )
        forward = -1;

    if ( forward ) {
        transform.Translate(
            Time.deltaTime * forward * (scrollSpeed + (y - minCamHeight)/2) * System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y) * System.Math.PI/180),
            0,
            Time.deltaTime * forward * (scrollSpeed + (y - minCamHeight)/2) * System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y) * System.Math.PI/180),
            Space.World
        );
    }

    // Zoom in(>0) and out (<0)
    var zoom = Input.GetAxis("Mouse ScrollWheel");
    zoom = zoom ? ( zoom < 0 ? -1 : 1 ) : 0;
    // Only zoom if we're not on min level
    if ( zoom && (( zoom > 0 && minCamHeight < y ) || ( zoom < 0 && y < maxCamHeight )) )
        transform.Translate((zoom * zoomSpeed * Camera.main.transform.position.y / maxCamHeight) * Vector3.forward);

    // ROTATION
    if (Input.GetMouseButton(1)) {
        var h : float = rotationSpeed * Input.GetAxis("Mouse X");
        var v : float = -rotationSpeed * Input.GetAxis("Mouse Y");
        transform.Rotate(0, h, 0, Space.World);
        transform.Rotate(v, 0, 0);

        var x = Camera.main.transform.rotation.eulerAngles.x;
        if(x < 0 || x > 80)
             transform.Rotate(-v, 0, 0);
    }
    // Following terraint (optional)
    // if(Terrain.activeTerrain.SampleHeight(camera.transform.position)+70<minCamHeight)
    //     Camera.main.transform.position.y = minCamHeight;
    // if(Terrain.activeTerrain.SampleHeight(camera.transform.position)+70>maxCamHeight)
    //     Camera.main.transform.position.y = maxCamHeight;
}
