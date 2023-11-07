<script>
    import Materialize from "materialize-css";
    
    import TextInput from "../../Modules/TextInput.svelte";
    import PasswordInput from "../../Modules/PasswordInput.svelte";
    import Button from "../../Modules/Button.svelte";
    import Alert from "../../Modules/Alert.svelte";

    let username = "";
    let password = "";
    let alert_text = "";
    let alert_is_visible = false;
    
    let login_button_handler = function() {
        alert_is_visible = false;
        let encodedLogin = "Basic " + btoa(username + ":" + password);
        console.log(encodedLogin);

        let httpRequest = new XMLHttpRequest();
        let url = '/login';
        httpRequest.open("GET", url);
        httpRequest.onloadend = function (e) {
            switch (httpRequest.status) {
                case 200:
                    let session = JSON.parse(httpRequest.response).sessionId;
                    localStorage.setItem("sessionToken", session);
                    window.location.href = "/chat.html"
                    break;
                case 401:
                    password = '';
                    alert_text = "Incorrect user or password";
                    alert_is_visible = true;
                    break;
                case 500:
                    alert_text = "Our server is out";
                    alert_is_visible = true;
            }
        };
        httpRequest.setRequestHeader(
            "Authorization",
            encodedLogin,
        );
        httpRequest.send();
    };
    
    let signup_button_handler = function() {
        window.location.href = '/signup.html';
    };
</script>

<div class="container app-body">
    <h1 class="row">Aravipac Chat Corp</h1>
    <h2 class="row">Login</h2>
    <div class="row">
        <TextInput
                bind:input_text={username}
                id="inUser"
                label_text="User"
                placeholder="User">
        </TextInput>
        <PasswordInput
                bind:input_text={password}
                id="inPassword"
                label_text="Password"
                placeholder="Password">
        </PasswordInput>
        <div class="row">
            <div class="col s10 offset-s1">
                <Alert bind:alert_text={alert_text} is_visible={alert_is_visible}></Alert>
            </div>
        </div>
        <div class="row">
            <div class="col s2">
                <Button button_label="Login" on:click={login_button_handler}></Button>
            </div>
            <div class="col s2 offset-s8">
                <Button button_label="Sign Up" on:click={signup_button_handler}></Button>
            </div>
        </div>
    </div>
</div>

<style>
    .app-body {
        overflow-y: hidden;
    }
    h1 {
        font-size: 30px;
        text-align: center;
        margin-bottom: 10%;
    }
    h2 {
        font-size: 24px;
        text-align: left;
    }
</style>
