(async function () {
    let btnSearch = document.querySelector('.btnSearch'),
        txtSearch = document.querySelector('.txtSearch')

    function search() {
        if (!txtSearch.value)
            return

        window.location = `/search/${encodeURIComponent(txtSearch.value)}`
    }

    txtSearch.addEventListener("keydown", (event) => {
        if (event.keyCode === 13) 
            search()
    })

    btnSearch.addEventListener('click', search, false)
})()