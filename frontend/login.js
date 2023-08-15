function login() {
    let user = document.getElementById("inUser").value;
    let password = document.getElementById("inPassword").value;

    console.log(user, password);
    let encodedLogin = "Basic " + btoa(user + ":" + password);

    console.log(encodedLogin);

    const httpRequest = new XMLHttpRequest();
    const url = 'http://localhost:5140/login';
    httpRequest.open("GET", url);
    httpRequest.onreadystatechange = function (e) {
        console.log(httpRequest.responseText)
    };
    httpRequest.setRequestHeader(
        "Authorization",
        encodedLogin,
    );
    httpRequest.send();
}