var DetailsApp = DetailsApp || {};

var connection;
const max_num_messages = 100;

var msgapp = new Vue({
    el: '#msgapp',
    data: {
        items: []
    }
});

var grapp = new Vue({
    el: '#grapp',
    data: {
        userStyle: { backgroundColor: "green" },
        schedStyle: { backgroundColor: "black" },
        lossStyle: { backgroundColor: "red" },
        parStyle: { backgroundColor: "green" },
        contrStyle: { backgroundColor: "black" },
        sessStyle: { backgroundColor: "red" },
        errStyle: { backgroundColor: "green" },
        mktStyle: { backgroundColor: "black" },
        error_nbr: 0,
        Id: 0
    }
});

var edapp = new Vue({
    el: '#edapp',
    data: {
        Currency: "",
        UPL: 0,
        RPL: 0,
        Info: ""
    }
});

var msapp = new Vue({
    el: '#msapp',
    data: {
        items: []
    }
});



DetailsApp.start = function () {
    connection = new window.signalR.HubConnectionBuilder()
        .withUrl("/dataHub")
        .build();

    connection.on('updateStates', (sender, message) => {
        var state_object = JSON.parse(message);
        this.set_msgapp_data(state_object);
        this.set_grapp_data(state_object);
        this.set_edapp_data(state_object);
        this.set_msapp_data(state_object);
    });

    $(window).on("beforeunload",
        function () { connection.invoke('leaveAccount', window.accountId); });

    connection.start()
        .then(() => connection.invoke('joinAccount', window.accountId));
}

DetailsApp.set_msgapp_data = function (state_object) {
    var second_array = state_object.MessagesToShow;
    second_array.reverse();
    var merged = second_array.concat(msgapp.items);
    var length = merged.length;
    if (length > max_num_messages) { merged = merged.slice(0, max_num_messages); }
    msgapp.items = merged;
};

DetailsApp.set_grapp_data = function (state_object) {
    let objKeys = Object.keys(state_object.RestrictionDetails);
    objKeys.forEach(key => {
        let value = state_object.RestrictionDetails[key];
        var v = "green";
        if (value === 1) v = "red";
        else if (value === 2) v = "black";
        grapp[key].backgroundColor = v;
    });
    grapp.error_nbr = state_object.DayErrorNbr;
    grapp.Id = state_object.Id;
}

DetailsApp.set_edapp_data = function (state_object) {
    edapp.Currency = state_object.Currency;
    edapp.UPL = state_object.UPL;
    edapp.RPL = state_object.RPL;
    edapp.Info = state_object.Info;
}

DetailsApp.set_msapp_data = function (state_object) {
    msapp.items = [];
    state_object.MktOrStrategies.forEach(ms => {
        let objKeys = Object.keys(ms.RestrictionDetails);
        var summary = {
            Id: ms.Id,
            Name: ms.Name,
            Type: ms.IsMarket?2:3,
            UPL: ms.UPL,
            RPL: ms.RPL,
            Position: ms.Position,
            SessionResult: ms.SessionResult,
            Info: ms.Info
        };
        objKeys.forEach(key => {
            let value = ms.RestrictionDetails[key];
            var v = "green";
            if (value === 1) v = "red";
            else if (value === 2) v = "black";
            summary[key] = { backgroundColor: v };
        });
        msapp.items.push(summary);
    });
}

DetailsApp.Message = function(source_id, command_id, source_type) {
    connection.invoke('postUserCommand', [source_id, command_id, source_type]);
}