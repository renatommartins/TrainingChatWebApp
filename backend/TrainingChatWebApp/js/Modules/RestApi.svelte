<script>
    export const Login = function(username, password, success_callback, failure_callback) {
        let encodedLogin = "Basic " + btoa(username + ":" + password);

        let httpRequest = new XMLHttpRequest();
        let url = '/login';
        httpRequest.open("GET", url);
        httpRequest.onloadend = function (e) {
            switch (httpRequest.status) {
                case 200:
                    success_callback({
                        statusCode: httpRequest.status,
                        session: JSON.parse(httpRequest.response).sessionId,
                    }); 
                    break;
                default:
                    failure_callback({
                        statusCode: httpRequest.status,
                    }); 
                    break;
            }
        };
        httpRequest.setRequestHeader(
            "Authorization",
            encodedLogin,
        );
        httpRequest.send();
    };
    
    export const Signup = function (name, username, password, success_callback, failure_callback) {
        let httpRequest = new XMLHttpRequest();
        let url = '/signup';
        let body = JSON.stringify({Name: name, Username: username, Password: password});
        
        httpRequest.open("POST", url);
        httpRequest.setRequestHeader("Content-type","application/json");
        httpRequest.onloadend = function (e) {
            switch (httpRequest.status) {
                case 200:
                    success_callback({
                        statusCode: httpRequest.status,
                    });
                    break;
                default:
                    failure_callback({
                        statusCode: httpRequest.status,
                    });
                    break;
            }
        };
        httpRequest.send(body);
    }
</script>
