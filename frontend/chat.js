document.addEventListener('DOMContentLoaded', function () {
	var elems = document.querySelectorAll('.modal');
	var instances = M.Modal.init(elems);
	window.modalInstances = instances;
});

function Initialize(a) {
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
		console.log('Conectou');
		RequestListChatRoom(websocket);
	});
	websocket.addEventListener("message", (event) => {
		console.log(event);
		var response = JSON.parse(event.data);
		console.log(response);

		switch (response.Type) {
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
				ResponseLeaveChatRoom(response.Data);
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
	websocket.addEventListener("close", (event) => {
		console.log("Fechou");
	});
}
function ResponseCreateChatRoom() {
	let chatRoomListElement = document.getElementById("idChatRoomListDiv");
	let joinedChatRoomElement = document.getElementById("idJoinedChatRoom");
	let chatRoomMessagesElement = document.getElementById("idChatRoomMessages");
	let chatRoomUsersElement = document.getElementById("idChatRoomUsers");
	let nameRoom = document.getElementById("inRoomName");
	let outAlertRoom = document.getElementById("outAlertRoom");
	window.modalInstances.forEach(element => {
		if (element.id == "createRoomModal") {
			element.close();
		}
	});
	outAlertRoom.innerHTML = "";
	chatRoomListElement.classList.add("hide");
	joinedChatRoomElement.classList.remove("hide");
	nameRoom.value = "";
	let newUserElement = chatRoomUsersElement.insertRow();
	newUserElement.innerHTML = "User"
}
function ResponseLeaveChatRoom() {
	let chatRoomListElement = document.getElementById("idChatRoomListDiv");
	let joinedChatRoomElement = document.getElementById("idJoinedChatRoom");


	chatRoomListElement.classList.remove("hide");
	joinedChatRoomElement.classList.add("hide");
}
function ResponseListChatRoom(roomArray) {
	let tableElement = document.getElementById("idChatRoomListTable");
	let tableBodyElement = tableElement.getElementsByTagName("tbody")[0];
	tableBodyElement.innerHTML = "";
	for (let i = 0; i < roomArray.length; i++) {
		console.log(roomArray[i]);
		let newElement = tableBodyElement.insertRow();
		newElement.innerHTML = roomArray[i].Name;
		newElement.onclick = function () { RequestJoinChatRoom(window.websocket, roomArray[i].Name, roomArray[i].Id) };
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
		if (tableBodyElement.rows[i].id == userName) {
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

function RequestCreateChatRoom() {
	let nameRoom = document.getElementById("inRoomName").value;
	let outAlertRoom = document.getElementById("outAlertRoom");
	if (nameRoom.length == 0) {
		outAlertRoom.innerHTML = "The name can't be empty"
		return
	}
	websocket.send(
		JSON.stringify(
			{
				Type: "CreateChatRoom",
				Data: { Name: nameRoom }
			}));
	console.log(websocket);
	console.log("Sala " + nameRoom + " criada!");

}

function RequestLeaveChatRoom() {
	websocket.send(
		JSON.stringify(
			{
				Type: "LeaveChatRoom",
				Data: {}
			}
		)
	);
}

function RequestListChatRoom(websocket) {
	websocket.send(
		JSON.stringify(
			{
				Type: "ListChatRoom",
				Data: {}
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
	console.log("cliquei no " + name + " / ID: " + id);
}