document.getElementById("searchForm").addEventListener("submit", function (event) {
    event.preventDefault();

    const numeroMatricula = document.getElementById("numeroMatricula").value;
    const resultList = document.getElementById("resultList");
    resultList.innerHTML = "<li>Processando...</li>";

    fetch(`/api/extratoclube/consultarbeneficios`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            cpf: numeroMatricula, 
            usuario: "testekonsi",
            senha: "testekonsi"
        })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Erro na consulta.");
            }
            return response.json();
        })
        .then(data => {
            resultList.innerHTML = "";
            if (data.length === 0) {
                resultList.innerHTML = "<li>Nenhuma informação encontrada</li>";
            } else {
                data.forEach(beneficio => {
                    const li = document.createElement("li");
                    li.innerText = beneficio;
                    resultList.appendChild(li);
                });
            }
        })
        .catch(error => {
            console.error(error);
            resultList.innerHTML = "<li>Erro na consulta. Tente novamente mais tarde.</li>";
        });
});
