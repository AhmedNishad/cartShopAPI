// Auto Complete


function autoCompletionFunctionality() {
    var autoCompleteElements = document.getElementsByClassName("autocomplete")
    var containingList = []
    for (var i = 0; i < autoCompleteElements.length; i++) {
        autoCompleteElements[i].autocomplete = false
        var hiddenSelect = autoCompleteElements[i].children[0]
        hiddenSelect.selectedIndex = -1
        var inputField = autoCompleteElements[i].children[1]
        var droppingDownElement = autoCompleteElements[i].children[2]
        inputField.addEventListener('input', (e) => {
            e.target.className = "form-control"

            var dropdown = e.target.parentElement.children[2]
            var inputString = e.target.value
            containingList = []
            var localHiddenSelect = e.target.parentElement.firstElementChild
            if (inputString == "") {
                dropdown.hidden = true
                localHiddenSelect.selectedIndex = -1
            } else {
                var localList = []
                for (var j = 0; j < localHiddenSelect.children.length; j++) {
                    if (localHiddenSelect.children[j].text.toLowerCase().startsWith(inputString.toLowerCase())) {
                        localList.push(localHiddenSelect.children[j].cloneNode(true))
                    }
                }
                if (localList.length == 0) {
                    e.target.value = ""
                    e.target.placeholder = "Not found"
                    e.target.className = " autocomplete-not-found"
                    e.target.parentElement.children[0].selectedIndex = -1;
                }
                containingList = localList
                dropdown.hidden = false
            }
            // Update Visible Field
            if (dropdown.children.length > 0) {
                //for (var g = 0; g < dropdown.options.length-1; g++) {
                //    dropdown.options.remove(1)
                //}
                while (dropdown.options.length > 1) {
                    dropdown.options.remove(1)
                }
            }
            if (containingList.length > 0) {
                for (var f = 0; f < containingList.length; f++) {

                    dropdown.options.add(containingList[f])
                }
                if (containingList.length == 1) {
                    // Assign placeholder to selected name and select the hidden elements value
                    //e.target.parentElement.children[2]
                    dropdown.selectedIndex = 1
                    e.target.parentElement.children[0].value = dropdown.value
                    e.target.placeholder = containingList[0].innerText.trim()
                    e.target.className = " autocomplete-found"
                    // console.log(e.target.parentElement.children[0].options[dropdown.selectedIndex])
                    e.target.parentElement.children[0].value = dropdown.value
                    e.target.value = ""
                    // e.target.parentElement.children[0].selectedIndex = 
                } else {
                    dropdown.selectedIndex = 1
                    e.target.parentElement.children[0].value = dropdown.value

                }
                
            }
            containingList = []
        })

        droppingDownElement.addEventListener('change', (e) => {
            
            e.target.parentElement.children[0].value = e.target.value
            e.target.parentElement.children[1].value = e.target.selectedOptions[0].innerText.trim()
            e.target.parentElement.children[1].className = "autocomplete-found"
        })
    }

}

autoCompletionFunctionality()