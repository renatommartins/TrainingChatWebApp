function login() {
    let user = document.getElementById("inUser").value;
    let passwordElement = document.getElementById("inPassword");
    let password = passwordElement.value
    let outAlert = document.getElementById("outAlert");
    let cardAlert = document.getElementById("cardAlert")
    console.log(user, password);
    let encodedLogin = "Basic " + btoa(user + ":" + password);

    console.log(encodedLogin);

    let httpRequest = new XMLHttpRequest();
    let url = 'http://localhost:5140/login';
    httpRequest.open("GET", url);
    httpRequest.onloadend = function (e) {
        switch (httpRequest.status) {
            case 200:
                let session = JSON.parse(httpRequest.response).sessionId;
                myStorage = localStorage;
                localStorage.setItem(myStorage, session);
                window.location.href = "/frontend/chat.html"
                break;
            case 401:
                passwordElement.value = '';
                outAlert.innerHTML = "Incorrect user or password";
                cardAlert.classList.remove("hide");
                break;
            case 500:
                outAlert.innerHTML = "Our servers are out";
                cardAlert.classList.remove("hide");
        }
    };
    httpRequest.setRequestHeader(
        "Authorization",
        encodedLogin,
    );
    httpRequest.send();
}