document.addEventListener('DOMContentLoaded', function() {
	var elems = document.querySelectorAll('.modal');
	var instances = M.Modal.init(elems);
  });

function Initialize (a){
	let sessionToken = localStorage.getItem("sessionToken");
	console.log(sessionToken);
	
	
	let getuserRequest = new XMLHttpRequest();
	getuserRequest.open("GET", 'http://localhost:5140/user');
	getuserRequest.onloadend = function (e) {
			switch (getuserRequest.status) {
				case 200:
					let userData = JSON.parse(getuserRequest.response);
					console.log(userData);
					ChatConnection(userData);
					break;
				case 401:
					window.location.href = 'index.html'
					break;
				case 500:
					console.log("Our server is out");
					break;
			}
	};
	getuserRequest.setRequestHeader("Authorization", 'Bearer ' + sessionToken,);
	getuserRequest.send();
};

function ChatConnection(userData) {
	let sessionToken = localStorage.getItem("sessionToken");
	let websocket = new WebSocket('ws://localhost:5140/chat-ws', sessionToken);
	window.websocket = websocket;
	websocket.addEventListener("open", (event) => {
		console.log('conecto caraio');
		RequestListChatRoom(websocket);
	});
	websocket.addEventListener("message", (event) => {
		console.log(event);
		var response = JSON.parse(event.data);
		console.log(response);

		switch(response.Type) {
			case "ResponseCreateChatRoom":
				ResponseCreateChatRoom(response.Data);
				console.log("ResponseCreateChatRoom");
				break;
			case "ResponseListChatRoom":
				console.log("ResponseListChatRoom");
				ResponseListChatRoom(response.Data.ChatRooms);
				break;
			case "ResponseJoinChatRoom":
				console.log("ResponseJoinChatRoom");
				ResponseJoinChatRoom(response.Data);
				break;
			case "ResponseLeaveChatRoom":
				console.log("ResponseLeaveChatRoom");
				break;
			case "ReceiveChatRoomMessage":
				console.log("ReceiveChatRoomMessage");
				break;
			case "UserJoinedChatRoom":
				console.log("UserJoinedChatRoom");
				NotificationUserJoined(response.Data.UserName);
				break;
			case "UserLeftChatRoom":
				console.log("UserLeftChatRoom");
				NotificationUserLeftChatRoom(response.Data.UserName);
				break;
			case "ResponseSendChatRoomMessage":
				console.log("ResponseSendChatRoomMessage");
				break;
		}
	});
	websocket.addEventListener("close", (event) =>{
		console.log("porra, fechou");
	});
}
function ResponseCreateChatRoom(){
	let chatRoomListElement = document.getElementById("idChatRoomListDiv");
	let joinedChatRoomElement = document.getElementById("idJoinedChatRoom");
	let chatRoomMessagesElement = document.getElementById("idChatRoomMessages");
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");

	chatRoomListElement.classList.add("hide");
	joinedChatRoomElement.classList.remove("hide");

	let newUserElement = chatRoomUsersElement.insertRow();
	newUserElement.innerHTML = "Felipe"
}

function ResponseListChatRoom(roomArray) {
	let tableElement = document.getElementById("idChatRoomListTable");
	let tableBodyElement = tableElement.getElementsByTagName("tbody")[0];
	tableBodyElement.innerHTML = "";
	for(let i = 0; i < roomArray.length; i++) {
		console.log(roomArray[i]);
		let newElement = tableBodyElement.insertRow();
		newElement.innerHTML = roomArray[i].Name;
		newElement.onclick = function() { RequestJoinChatRoom(window.websocket, roomArray[i].Name, roomArray[i].Id) };
	}
}

function ResponseJoinChatRoom(chatRoomData) {
	let chatRoomListElement = document.getElementById("idChatRoomListDiv");
	let joinedChatRoomElement = document.getElementById("idJoinedChatRoom");
	let chatRoomMessagesElement = document.getElementById("idChatRoomMessages");
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");

	chatRoomListElement.classList.add("hide");
	joinedChatRoomElement.classList.remove("hide");

	chatRoomData.UserNames.forEach((userName) => {
		let newUserElement = chatRoomUsersElement.insertRow();
		newUserElement.id = userName;
		newUserElement.innerHTML = userName;
	});
}

function NotificationUserLeftChatRoom(userName) {
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");
	let tableBodyElement = chatRoomUsersElement.getElementsByTagName("tbody")[0];

	for (let i = 0; i < tableBodyElement.rows.length; i++) {
		console.log(tableBodyElement.rows[i].id);
		if(tableBodyElement.rows[i].id == userName){
			tableBodyElement.deleteRow(i);
			break;
		}
	}
}

function NotificationUserJoined(userName) {
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");

	let newUser = chatRoomUsersElement.insertRow();
	newUser.id = userName;
	newUser.innerHTML = userName;
}

// Messages to server.

function RequestCreateChatRoom(){
	let nameRoom = document.getElementById("inRoomName").value;
	websocket.send(
		JSON.stringify(
			{
				Type: "CreateChatRoom",
				Data: {Name: nameRoom}
			}));

	console.log(websocket);
	console.log("Sala " + nameRoom + " criada!");

}

function RequestListChatRoom (websocket) {
	websocket.send(
		JSON.stringify(
			{
				Type: "ListChatRoom",
				Data:{}
			}));
}

function RequestJoinChatRoom(websocket, name, id) {
	websocket.send(
		JSON.stringify(
			{
				Type: "JoinChatRoom",
				Data: {
					Id: id,
				}
			}));
	console.log(websocket);
	console.log ("cliquei nu " + name + " / ID: " + id);
}