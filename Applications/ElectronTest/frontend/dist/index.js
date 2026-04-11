(async () => {

    document.getElementById("target").textContent = await sendTestRequest("Frontend", "Hey");

})();

async function sendTestRequest(name, value) {
    return (await backend.request({ testRequest: { name: name, value: value } })).testResponse.value;
}