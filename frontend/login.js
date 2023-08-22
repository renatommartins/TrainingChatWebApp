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
                console.log("Enviar para a página correta");
                let session = JSON.parse(httpRequest.response).sessionId;
                myStorage = localStorage;
                localStorage.setItem(myStorage, session);
                window.location.href = "/frontend/chat.html"
                break;
            case 401:
                passwordElement.value = '';
                outAlert.innerHTML = "Incorrect user or password";
                cardAlert.classList.remove("hide");
                console.log("Erro de senha ou usuário");
                break;
            case 500:
                console.log("servidor bunda mole, tente novamente");
        }
    };
    httpRequest.setRequestHeader(
        "Authorization",
        encodedLogin,
    );
    httpRequest.send();
}