"use strict";


var connection = new signalR.HubConnectionBuilder().withUrl("/ticTacToeHub").build();

var btnSearch = $("#btnSearch");
var nickInput = $("#nickname");

var lobby = $("#lobby");
var roomsList = $("#rooms");

var game = $("#game");
var markerInfo = $("#markerInfo");
var info = $("#info");
var field = $("#field");

//Start connection
connection.start().then(() => {

    lobby.show();
    roomsList.hide();
    game.hide();

}).catch(error => {
    console.error("Error while establishing connection:", error);
});

//Enter nickname and search for game
btnSearch.click(function () {

    var nick = nickInput.val();
    if (nick.length == 0) {
        alert("Enter nickname");
        return;
    }
    roomsList.show();
    connection.invoke("OnSearch", nick)
})

//Display rooms 
connection.on("Search", players => {
    roomsList.empty();

    players.forEach(player => {
        if (player.id != connection.connectionId) {
            $("<div>")
                .addClass("list-group-item list-group-item-action d-flex justify-content-between align-items-center")
                .attr("id", "room" + player.id)
                .data("name", player.name)
                .data("id", player.id)
                .append(
                    $("<div>").text(`${player.id}: ${player.name}`),
                    $("<button>")
                        .addClass("btn btn-primary btn-sm invite-btn")
                        .text("Invite")
                )
                .appendTo(roomsList);
        }
    });
});

//Event handler for invite buttons
roomsList.on('click', '.invite-btn', function () {
    var playerName = $(this).parent().data("name");
    var playerId = $(this).parent().data("id");
    if (confirm("Do you want to invite " + playerName + " to play?")) {
        connection.invoke("InviteToPlay", playerId);
    }
});


//Remove room from list on player disconnect or on game join
connection.on("RemoveFromList", (...playerIds) => {
    playerIds.forEach(playerId => {
        $("#room" + playerId).remove();
    });
});

//Prompt user to accept or reject invitation
connection.on("ReceiveInvitation", (inviter) => {
    var confirmed = confirm("You received an invitation from " + inviter.name + ". Do you want to play?");
    if (confirmed) {
        connection.invoke("AcceptInvitation", inviter.id);
    } else {
        connection.invoke("DeclineInvitation", inviter.id);
    }
});

//Handle reject
connection.on("InvitationDeclined", () => {
    alert("Your invite was declined");
});

//Game start

connection.on("InvitationAccepted", (isMarking, mark) => {
    

    lobby.hide();
    roomsList.hide();
    game.show();

    let markSymbol = (mark === -1) ? "X" : "O";
    let markColor = (mark === -1) ? "red" : "green";

    // Set the markerInfo text with the appropriate color
    markerInfo.html(`You are playing as <strong style="color: ${markColor};">${markSymbol}</strong>`);

    

    info.text(isMarking ?
        "Your move" :
        "Waiting for opponent move...");

    //generate field
    for (let i = 0; i < 3; i++) {
        field.append('<div class="row d-flex justify-content-center"></div>');

        for (let j = 0; j < 3; j++) {
            $(".row:last").append(`
                <div class="col">
                    <button id="${i * 3 + j}" data-index=${i * 3 + j} class="btn btn-primary"></button>
                </div>
            `);
        }
    }
});

field.on('click', 'button', function () {

    var buttonIndex = $(this).data('index');

    connection.invoke("MakeMark", buttonIndex)
});

connection.on("Mark", function (index, marker, isMarking) {
    var button = $("#" + index);
    var markerClass = marker == -1 ? "crossMarker" : "circleMarker";
    button.addClass(markerClass);
    button.attr("disabled", true);

    info.text(isMarking ?
        "Your move":
        "Waiting for opponent move...");


});

connection.on("GameEnd", winner => {
    const headerText = winner ? (connection.connectionId == winner.id
        ? 'You won!'
        : 'You lost!')
        : 'Draw!';
    const leadText = winner ? (connection.connectionId == winner.id
        ? `Congratulations, ${winner.name}! You're the champion of this tic-tac-toe match!`
        : `Hard luck this time, but keep playing! You lost to ${winner.name} in this round of tic-tac-toe.`)
        : 'The game ended in a draw. Play again!';

    const headerClass = winner ? (connection.connectionId == winner.id
        ? 'text-success'
        : 'text-danger')
        : 'text-info';

    const header = $('<h1>').addClass('display-4').addClass(headerClass).text(headerText);
    const lead = $('<h2>').addClass('lead').text(leadText);

    const backButton = $('<button>')
        .addClass('btn btn-secondary mt-3')
        .attr('id', "btnBack")
        .text('Search games');


    info.empty().append(header).append(lead).append(backButton);
});

connection.on("Surrender", () => {

    const header = $('<h1>').addClass('display-4').addClass('text-success').text('You won!');
    const lead = $('<h2>').addClass('lead').text('Your opponent has surrendered, recognizing your victory.');

    const backButton = $('<button>')
        .addClass('btn btn-secondary mt-3')
        .attr('id', "btnBack")
        .text('Search games');

    info.empty().append(header).append(lead).append(backButton);
});

info.on("click", "#btnBack", function () {
    game.hide();

    field.empty();
    info.empty();

    lobby.show();
    roomsList.show();



    connection.invoke("LeaveGame");
    connection.invoke("OnSearch", nickInput.val());
});



connection.on("OpponentLeft", () => {
    alert("Your opponent left");
});