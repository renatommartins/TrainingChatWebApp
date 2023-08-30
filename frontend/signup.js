function signup() {
    let userElement = document.getElementById("inUser");
    let user = userElement.value;
    let nameElement = document.getElementById("inName");
    let name = nameElement.value;
    let passwordElement = document.getElementById("inPassword");
    let password = passwordElement.value
    let confirmPasswordElement = document.getElementById("inConfirmPassword");
    let confirmPassword = confirmPasswordElement.value
    let outAlert = document.getElementById("outAlert");
    let successCreate = document.getElementById("successCreate");
    let cardAlert = document.getElementById("cardAlert");
    let cardSuccess = document.getElementById("cardSuccess");
    let signUpButton = document.getElementById("signUpButton");

    console.log(name, user, password, confirmPassword);

    if(password != confirmPassword)
    {
        passwordElement.value = '';
        confirmPasswordElement.value = '';
        outAlert.innerHTML = "Different password";
        cardAlert.classList.remove("hide");
        return;
    }

    let httpRequest = new XMLHttpRequest();
    let url = 'http://localhost:5140/signup';
    let body = JSON.stringify({Name: name, Username: user, Password: password});
    console.log(body);
    httpRequest.open("POST", url);
    httpRequest.setRequestHeader("Content-type","application/json");
    httpRequest.onloadend = function (e) {
        switch (httpRequest.status) {
            case 200:
                console.log("Sucesso");
                successCreate.innerHTML = "User created successfully\nRedirecting to Login Page";
                cardSuccess.classList.remove("hide");
                signUpButton.classList.add("disabled");
                nameElement.disabled = true; 
                userElement.disabled = true; 
                passwordElement.disabled = true; 
                confirmPasswordElement.disabled = true; 

                setTimeout(function (){                    
                    window.location.href = "/index.html"
                }, 5000)
                break;
            case 409:
                passwordElement.value = '';
                outAlert.innerHTML = "User already exists";
                cardAlert.classList.remove("hide");
                console.log("Usuário existente");
                break;
            case 500:
                console.log(".");
        }
    };
    httpRequest.send(body);
}

