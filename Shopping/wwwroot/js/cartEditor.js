var datePicker = document.getElementById("date-picker");
var date = new Date();
datePicker.value = `${date.getFullYear()}-${date.getMonth() + 1}-${date.getDate()}`;
var productPicker = document.getElementById("product-picker");
var quantityInput = document.getElementById("quantity-input");
var quantityOutput = document.getElementById("quantity-output");
var addLineItem = document.getElementById("add-line-item");
var lineItemContainer = document.getElementById("line-item-container");
var totalOutput = document.getElementById("total-output");
var idCounter = 0;
var submitButton = document.getElementById("submit-button");

submitButton.disabled = true;

addLineItem.disabled = true
quantityInput.addEventListener('input', (e) => {
    quantityInput.max = productPicker.options[productPicker.selectedIndex].getAttribute('data-unit-quantity')
    if (e.target.value == "" || parseInt(e.target.value) < 1 || parseInt(e.target.value) > quantityInput.max) {
        addLineItem.disabled = true
    } else {
        addLineItem.disabled = false
    }
})

function addFunctionality() {
    if (lineItemContainer.children.length ) {
        let tableRows = lineItemContainer.children
        for (let i = 0; i < tableRows.length; i++) {
            console.log(tableRows[i].children[3].firstElementChild)
            tableRows[i].children[3].firstElementChild.addEventListener('click', removeLineItem);
        }
    }
}
addFunctionality()

addLineItem.addEventListener('click', e => {
    e.preventDefault();
    var lineItemElement = document.createElement("tr");
    var productIdElement = document.createElement("td");
    productIdElement.innerHTML = `<input type='hidden' value='${productPicker.value}' name='[${idCounter}].product.id' /> 
                                    ${productPicker.options[productPicker.selectedIndex].getAttribute('data-product-name')}`

    var quantityElement = document.createElement("td");
    quantityElement.innerHTML = `<input type="hidden" value="${parseInt(quantityInput.value)}" name="[${idCounter}].quantity" /> 
                                ${quantityInput.value}`

    var totalElement = document.createElement("td");

    var lineTotal = (parseInt(productPicker.options[productPicker.selectedIndex].getAttribute('data-unit-price')) * parseInt(quantityInput.value));

    totalElement.innerHTML = `<input type="hidden" value="${lineTotal}" name="[${idCounter}].total" /> <h6>${lineTotal}</h6>`
    var removeElement = document.createElement("td");
    var removeButton = document.createElement("button")
    removeButton.innerText = "Remove"
    console.log(removeButton)
    removeButton.classList.add("btn", "btn-danger")

    productPicker.options[productPicker.selectedIndex].setAttribute("data-unit-quantity", parseInt(quantityInput.max) - parseInt(quantityInput.value))

    removeButton.addEventListener('click', removeLineItem)
    removeElement.appendChild(removeButton)

    lineItemElement.appendChild(productIdElement);
    lineItemElement.appendChild(quantityElement);
    lineItemElement.appendChild(totalElement);
    lineItemElement.appendChild(removeElement);
    lineItemContainer.appendChild(lineItemElement);
    idCounter++
    addIdCounter()
    updateTotal()
    checkSubmit()
    quantityInput.value = ""
    addLineItem.disabled = true
})

function updateTotal() {
    var total = 0;
    let lineItems = lineItemContainer.children
    for (let i = 0; i < lineItems.length; i++) {
        let child = lineItems[i]
        total += parseInt(child.children[2].firstElementChild.value)
    }
    totalOutput.innerText = total
}

function addIdCounter() {
    var orderId = 0
    var lineItems = lineItemContainer.children
    for (let i = 0; i < lineItems.length; i++) {
        var child = lineItems[i]
        grandChildren = child.children
        grandChildren[0].firstElementChild.setAttribute("name", `[${orderId}].product.id`)
        grandChildren[1].firstElementChild.setAttribute("name", `[${orderId}].quantity`)
        grandChildren[2].firstElementChild.setAttribute("name", `[${orderId}].total`)
        orderId++
    }
}

function removeLineItem(e) {
    e.preventDefault()
    var toRemove = e.target.parentElement.parentElement;
    productPicker.options[productPicker.selectedIndex].setAttribute("data-unit-quantity", parseInt(quantityInput.max) + parseInt(toRemove.children[1].firstElementChild.value))
    lineItemContainer.removeChild(toRemove);
    addIdCounter()
    updateTotal()
    checkSubmit()
}

function checkSubmit(){
    if (lineItemContainer.children.length > 0) {
        submitButton.disabled = false
    } else {
        submitButton.disabled = true
    }
}



