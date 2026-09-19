let editingDoctorId = 0;

const doctorFormHtml = `
<form id="doctor-form" class="card panel-body" novalidate>
<div class="form-grid">
<div class="field-group"><label>Name *</label><input name="name" maxlength="100"><div class="field-error" data-error-for="name"></div></div>
<div class="field-group"><label>Specialization *</label><input name="specialization" maxlength="100"><div class="field-error" data-error-for="specialization"></div></div>
<div class="field-group"><label>Department *</label><input name="department" maxlength="100"><div class="field-error" data-error-for="department"></div></div>
<div class="field-group"><label>Phone *</label><input name="phone" type="tel"><div class="field-error" data-error-for="phone"></div></div>
<div class="field-group"><label>Email</label><input name="email" type="email"><div class="field-error" data-error-for="email"></div></div>
<div class="field-group"><label>Experience (years)</label><input name="experienceYears" type="number" min="0" max="70"><div class="field-error" data-error-for="experienceYears"></div></div>
</div><div class="form-error"></div><div class="form-actions"><button type="button" class="btn btn-secondary" id="cancel-doctor">Cancel</button><button type="submit" class="btn btn-primary">Save doctor</button></div>
</form>`;

document.addEventListener("DOMContentLoaded", () => {
setPageContent(`<div class="page-header"><div><h1>Doctors</h1><p>Manage clinical staff and specialties.</p></div><button id="new-doctor" class="btn btn-primary">Add doctor</button></div><div id="doctor-form-container" class="hidden"></div><div class="panel"><div id="doctor-table" class="table-wrap"></div></div>`);
document.getElementById("new-doctor").addEventListener("click", () => openDoctorForm());
document.getElementById("doctor-form-container").addEventListener("submit", handleDoctorSubmit);
document.getElementById("doctor-form-container").addEventListener("click", event => { if (event.target.id === "cancel-doctor") closeDoctorForm(); if (event.target.dataset.editId) editDoctor(Number(event.target.dataset.editId)); if (event.target.dataset.deleteId) deleteDoctor(Number(event.target.dataset.deleteId)); });
loadDoctors();
});

function openDoctorForm(doctor = null) { editingDoctorId = doctor?.id || 0; const c = document.getElementById("doctor-form-container"); c.classList.remove("hidden"); c.innerHTML = doctorFormHtml; if (doctor) fillDoctorForm(doctor); c.scrollIntoView({behavior:"smooth"}); }
function closeDoctorForm() { editingDoctorId = 0; const c = document.getElementById("doctor-form-container"); c.classList.add("hidden"); c.innerHTML = ""; }
function fillDoctorForm(doctor) { const f=document.getElementById("doctor-form"); f.name.value=doctor.name||""; f.specialization.value=doctor.specialization||""; f.department.value=doctor.department||""; f.phone.value=doctor.phone||""; f.email.value=doctor.email||""; f.experienceYears.value=doctor.experienceYears ?? 0; }
async function handleDoctorSubmit(event) { event.preventDefault(); event.stopPropagation(); const f=event.target; clearFieldErrors(f); if(!validateRequired(f,["name","specialization","department","phone"])) return; const b=f.querySelector("button[type=submit]"); setButtonBusy(b,true); const payload={name:f.name.value.trim(),specialization:f.specialization.value.trim(),department:f.department.value.trim(),phone:f.phone.value.trim(),email:f.email.value.trim(),experienceYears:Number(f.experienceYears.value||0)}; try { if(editingDoctorId){await ApiClient.put(`doctors/${editingDoctorId}`,payload);showToast("Doctor updated successfully.");} else {await ApiClient.post("doctors",payload);showToast("Doctor added successfully.");} closeDoctorForm(); await loadDoctors(); } catch(error){if(error.details?.errors)showFieldErrors(f,error.details.errors);f.querySelector(".form-error").textContent=error.message;} finally{setButtonBusy(b,false);} }
async function loadDoctors(){try{const doctors=await ApiClient.get("doctors");document.getElementById("doctor-table").innerHTML=doctors.length?`<table><thead><tr><th>Number</th><th>Name</th><th>Specialization</th><th>Department</th><th>Experience</th><th>Phone</th><th>Actions</th></tr></thead><tbody>${doctors.map(d=>`<tr><td>${escapeHtml(d.doctorNumber)}</td><td><strong>${escapeHtml(d.name)}</strong></td><td>${escapeHtml(d.specialization)}</td><td>${escapeHtml(d.department)}</td><td>${d.experienceYears} yrs</td><td>${escapeHtml(d.phone)}</td><td><button class="btn btn-secondary" data-edit-id="${d.id}">Edit</button> <button class="btn btn-danger" data-delete-id="${d.id}">Delete</button></td></tr>`).join("")}</tbody></table>`:`<div class="empty-state"><strong>No doctors found</strong>Add a doctor to get started.</div>`;}catch(error){showToast(error.message,"error");}}
async function editDoctor(id){try{openDoctorForm(await ApiClient.get(`doctors/${id}`));}catch(error){showToast(error.message,"error");}}
async function deleteDoctor(id){if(!confirm("Delete this doctor?"))return;try{await ApiClient.delete(`doctors/${id}`);showToast("Doctor deleted successfully.");await loadDoctors();}catch(error){showToast(error.message,"error");}}
