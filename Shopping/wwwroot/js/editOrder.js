
var editableItems = document.getElementsByClassName("editable-item")
var editingFormField = document.getElementById("editing-form-field")
editingFormField.hidden = true
var editButton = document.getElementById('edit-line-item')
var submitButton = document.getElementById('submit-button')
var productPickerNew = document.getElementById('product-picker-new')
var productPickerEdit = document.getElementById('product-picker-edit')
var editFormContainer = document.getElementById('edit-form-container')

let showingEdit = false
var lineItemContainer = document.getElementById("line-item-container");
var addLineItem = document.getElementById("add-line-item");
var productInput = document.getElementById("product-input")
var productSelect = document.getElementById('product-select')

var productQuantity = document.getElementById("product-quantity") 
var idCounter = 0
var quantityInputNew = document.getElementById("quantity-input-new");
var quantityInputEdit = document.getElementById("quantity-input-edit");
quantityInputEdit.classList.add('col-3')

var selectedProduct;
var totalOutput = document.getElementById("total-output")
var removeButtons = document.getElementsByClassName('remove-button')


submitButton.disabled = true;

addLineItem.disabled = true




// Update autocompleted element ( For adding new items )
function getSelectedOptionElement(selectedId) {
    for (var p = 0; p < productPickerNew.options.length; p++) {
        var productElem = productPickerNew.options[p]
        if (productElem.value == selectedId) {
            return productElem
        }
    }
    return null
}


// Change selected product on product input
productInput.addEventListener('input', (e) => {
    editingFormField.hidden = true
    if (e.target.value.length > 1) {
        selectedProduct = getSelectedOptionElement(productPickerNew.value) //getSelectedOptionElement(productPickerNew.value)
    } else {
        selectedProduct = productSelect.options[0]
    }
}
)

// Change selected product on select event (Automated via element)
productSelect.addEventListener('select', (e) => {
    console.log(productSelect.value)
    selectedProduct = getSelectedOptionElement(productSelect.value)
    }
)

productSelect.addEventListener('change', (e) => {
    console.log(productSelect.value)
    selectedProduct = getSelectedOptionElement(productSelect.value)
    quantityInputNew.focus()
}
)

// Checks for line item and edit form visibility
quantityInputNew.addEventListener('input', (e) => {
    editingFormField.hidden = true

    quantityInputNew.max = selectedProduct.getAttribute('data-unit-quantity')
    if (e.target.value == "" || parseInt(e.target.value) < 1 || parseInt(e.target.value) > quantityInputNew.max) {
        addLineItem.disabled = true
    } else {
        addLineItem.disabled = false
    }
})

// Checks for edit button disabling
quantityInputEdit.addEventListener('input', (e) => {
    
    if (e.target.value == "" || parseInt(e.target.value) < 1 || parseInt(e.target.value) > quantityInputEdit.max) {
        editButton.hidden = true
    } else {
        editButton.hidden = false
    }
})



// Adding edit functionality on initialization
for (var i = 0; i < editableItems.length; i++) {
    editableItems[i].addEventListener("click", editingItem)
}

// Adding remove button functionality on initialization
for (var j = 0; j < removeButtons.length; j++) {
    removeButtons[j].addEventListener('click', removeLineItem);
}

// Add new line item with hidden inputs
addLineItem.addEventListener('click', e => {
    selectedProduct = getSelectedOptionElement(productPickerNew.parentElement.children[2].value)
    e.preventDefault();
    var lineItemElement = document.createElement("tr");
    var productIdElement = document.createElement("td");


    productIdElement.innerHTML = `<input type='hidden' value='${selectedProduct.value}' name='[${idCounter}].product.id' /> 
                                 <p>${selectedProduct.getAttribute('data-product-name')}</p>`

    var quantityElement = document.createElement("td");
    quantityElement.innerHTML = `<input type="hidden" value="${parseInt(quantityInputNew.value)}" name="[${idCounter}].quantity" /> 
                                <p>${quantityInputNew.value}</p>`

    var totalElement = document.createElement("td");

    var lineTotal = (parseInt(selectedProduct.getAttribute('data-unit-price')) * parseInt(quantityInputNew.value));

    var itemAlreadyExists = checkIfPreviouslySubmitted(selectedProduct.value, quantityInputNew.value, lineTotal)

    if (!itemAlreadyExists) {
        totalElement.innerHTML = `<input type="hidden" value="${lineTotal}" name="[${idCounter}].total" /> <p>${lineTotal}</p>`
        var removeElement = document.createElement("td");
        var removeButton = document.createElement("i")
        removeButton.classList.add("btn", "btn-danger", 'material-icons')
        removeButton.innerText = "remove"

        // Update Product quantity for element. Now obsolete because we're overriding updated elements
        //  productPicker.options[productPicker.selectedIndex].setAttribute("data-unit-quantity", parseInt(quantityInput.max) - parseInt(quantityInput.value))

        lineItemElement.addEventListener("click", editingItem)
        removeButton.addEventListener('click', removeLineItem)
        removeButton.classList.add('remove-button')
        removeElement.appendChild(removeButton)

        lineItemElement.setAttribute('data-product-quantity', selectedProduct.getAttribute('data-unit-quantity'))
        lineItemElement.setAttribute('data-product-name', selectedProduct.getAttribute('data-product-name'))
        lineItemElement.setAttribute('data-product-price', selectedProduct.getAttribute('data-unit-price'))
        lineItemElement.setAttribute('data-product-id', selectedProduct.value)

        lineItemElement.classList.add('editable-item')
        lineItemElement.classList.add('bg-newly-added')
        lineItemElement.appendChild(productIdElement);
        lineItemElement.appendChild(quantityElement);
        lineItemElement.appendChild(totalElement);
        lineItemElement.appendChild(removeElement);
        lineItemContainer.appendChild(lineItemElement);
        idCounter++
        addIdCounter()
        updateTotal()
        checkSubmit()
        quantityInputNew.value = ""
        productInput.value = ""
        addLineItem.disabled = true
    }
    productInput.focus()
})

// Handle when row is being edited. Update data attributes on edit form
function editingItem(e) {
    var tableRow = e.target.parentElement.parentElement
    if (tableRow.classList.contains('editable-item') && !e.target.classList.contains('remove-button')) {
        editingFormField.hidden = false

        editingFormField.children[0].firstElementChild.innerText = tableRow.getAttribute('data-product-name')
        editingFormField.setAttribute('price', tableRow.getAttribute('data-product-price'))


        editingFormField.setAttribute('data-product-id', tableRow.getAttribute('data-product-id'))
        tableRow.insertAdjacentElement('afterend', editingFormField)

        editingFormField.children[1].firstElementChild.value = ""
        editingFormField.children[1].firstElementChild.placeholder = parseInt(tableRow.children[1].innerText)
        editingFormField.children[1].firstElementChild.max = tableRow.getAttribute('data-product-quantity')
        editingFormField.children[1].firstElementChild.focus()
        editButton.hidden = true
        addIdCounter()
    } else {
        editingFormField.hidden = true
    }
}

editButton.addEventListener('click', editLineItem)

// Update hidden inputs and displays
function editLineItem(e) {
    e.preventDefault()
    
    var newlyAddedQuantity = e.target.parentElement.parentElement.children[1].firstElementChild.value
    var modifyingRow = findModifyingRow(editingFormField.getAttribute('data-product-id'))
    var newPrice = parseInt(newlyAddedQuantity) * parseInt(modifyingRow.getAttribute('data-product-price'))
    var quantityDisplay = modifyingRow.children[1].children[1]
    var priceDisplay = modifyingRow.children[2].children[1]
    var quantityInput = modifyingRow.children[1].firstElementChild
    var priceInput = modifyingRow.children[2].firstElementChild
    modifyingRow.classList.add('bg-newly-added')
    quantityDisplay.innerText = newlyAddedQuantity
    priceDisplay.innerText = newPrice
    priceInput.value = parseInt(newPrice)
    submitButton.disabled = false
    editingFormField.hidden = true;
    editingFormField.children[1].firstElementChild.value = ""
    editFormContainer.appendChild(editingFormField)
    quantityInput.value = parseInt(newlyAddedQuantity)
    updateTotal()   
    checkSubmit()
}

// Find corresponding form to get necessary data attributes
function findModifyingRow(productId) {
    for (var o = 0; o < lineItemContainer.children.length; o++) {
        var possibleRow = lineItemContainer.children[o]
        if (possibleRow.getAttribute('data-product-id') == productId) {

            return possibleRow
        }
    }
    return null
}

// Update names of hidden inputs to support model validation
function addIdCounter() {
    var orderId = 0
    var lineItems = lineItemContainer.children
    for (let i = 0; i < lineItems.length; i++) {
        var child = lineItems[i]
        if (child.getAttribute('id') != 'editing-form-field') {
            grandChildren = child.children
            grandChildren[0].firstElementChild.setAttribute("name", `[${orderId}].product.id`)
            grandChildren[1].firstElementChild.setAttribute("name", `[${orderId}].quantity`)
            grandChildren[2].firstElementChild.setAttribute("name", `[${orderId}].total`)
            if (grandChildren[0].children[2] != null) {
                grandChildren[0].children[2].setAttribute("name", `[${orderId}].id`)

            }
            orderId++

        }
    }
}


// If previously available in list. Override
function checkIfPreviouslySubmitted(productId, quantity, lineTotal) {
  
    for (var i = 0; i < lineItemContainer.children.length; i++) {
        var lineElement = lineItemContainer.children[i]
        var hiddenProductInput = lineItemContainer.children[i].firstElementChild.firstElementChild
        var productQuantityHiddenInput = lineItemContainer.children[i].children[1].firstElementChild
        var productQuantity = lineItemContainer.children[i].children[1].children[1]
        var lineTotalHiddenInput = lineItemContainer.children[i].children[2].firstElementChild
        var totalOfLine = lineItemContainer.children[i].children[2].children[1]
        if (hiddenProductInput.value == productId) {
            productQuantityHiddenInput.value = quantity
            productQuantity.innerText = quantity
            lineTotalHiddenInput.value = lineTotal
            totalOfLine.innerText = lineTotal;
            lineElement.classList.add('bg-newly-added')
            updateTotal()
            checkSubmit()
            quantityInputNew.value = ""
            console.log("Exists")
            return true
        }
    }
    return false;
}

// Update display of total at bottom of page
function updateTotal() {
    var total = 0;
    let lineItems = lineItemContainer.children
    for (let i = 0; i < lineItems.length; i++) {
        let child = lineItems[i]
        if (child.getAttribute('id') != 'editing-form-field')
        total += parseInt(child.children[2].firstElementChild.value)
    }
    totalOutput.innerText = total
}


// Remove entire row of inputs and displays. Update total and id counters
function removeLineItem(e) {
    e.preventDefault()
    var toRemove = e.target.parentElement.parentElement;
    productPickerNew.value = toRemove.getAttribute('data-product-id')
    editingFormField.hidden = true
    editFormContainer.append(editingFormField)
    lineItemContainer.removeChild(toRemove);
    addIdCounter()
    updateTotal()
    checkSubmit()
}

// Check if submit button should be visible
function checkSubmit() {
    console.log(lineItemContainer.children.length)
    if (lineItemContainer.children.length > 0) {
        submitButton.disabled = false
    } else {
        submitButton.disabled = true
    }
}
