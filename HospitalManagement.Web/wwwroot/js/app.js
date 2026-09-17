const app = {
  async request(url, options = {}) {
    const res = await fetch(url, { headers: { 'Content-Type': 'application/json', ...(options.headers || {}) }, ...options });
    let body = null;
    try { body = await res.json(); } catch {}
    if (!res.ok) {
      const err = new Error(body?.message || 'Something went wrong.');
      err.validationErrors = body?.errors || body?.validationErrors || null;
      throw err;
    }
    return body;
  },
  toast(message, type='success') {
    const host=document.getElementById('toastHost'); if(!host)return;
    const el=document.createElement('div'); el.className=`app-toast ${type}`; el.textContent=message; host.appendChild(el); setTimeout(()=>el.remove(),3500);
  },
  escape(value) { return String(value ?? '').replace(/[&<>'"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[c])); },
  setBusy(button,busy){if(!button)return;button.disabled=busy;button.dataset.originalText??=button.innerHTML;button.innerHTML=busy?'<span class="spinner-border spinner-border-sm me-2"></span>Saving...':button.dataset.originalText;},
  clearValidation(form){
    if(!form)return;
    form.querySelectorAll('.is-invalid').forEach(el=>el.classList.remove('is-invalid'));
    form.querySelectorAll('.field-error').forEach(el=>{el.textContent='';el.classList.remove('visible');});
  },
  fieldError(form, field, message){
    if(!form || !field || !message)return;
    const raw=String(field).replace(/^Model\./,'').replace(/^model\./,'');
    const wanted=raw.split('.').pop().replace(/[\[\]]/g,'').toLowerCase();
    const input=[...form.querySelectorAll('input[name],select[name],textarea[name],input[id],select[id],textarea[id]')].find(el=>{
      const key=String(el.name||el.id).replace(/^Model\./,'').split('.').pop().replace(/[\[\]]/g,'').toLowerCase();
      return key===wanted;
    });
    if(input){
      input.classList.add('is-invalid');
      const key=input.name||input.id;
      const boxes=[...form.querySelectorAll('.field-error')];
      const box=boxes.find(x=>String(x.getAttribute('data-error-for')||'').toLowerCase()===String(key).toLowerCase()) || boxes.find(x=>String(x.getAttribute('data-error-for')||'').toLowerCase()===wanted);
      if(box){box.textContent=message;box.classList.add('visible');}
    }
  },
  applyServerErrors(form,error){
    if(!form)return;
    this.clearValidation(form);
    const errors=error?.validationErrors;
    if(errors && typeof errors==='object'){
      Object.entries(errors).forEach(([field,value])=>{
        const message=Array.isArray(value)?value[0]:String(value);
        this.fieldError(form,field,message);
      });
      const first=form.querySelector('.is-invalid');
      if(first) first.focus();
      return;
    }
    this.toast(error?.message || 'Please check the entered information.','error');
  },
  validate(form){
    if(!form)return false;
    this.clearValidation(form);
    let ok=true;
    form.querySelectorAll('[required]').forEach(i=>{
      if(!String(i.value||'').trim()){
        i.classList.add('is-invalid');
        this.fieldError(form,i.name||i.id,'This field is required.');
        ok=false;
      }
    });
    form.querySelectorAll('input[type=email]').forEach(i=>{
      if(i.value && !/^\S+@\S+\.\S+$/.test(i.value)){
        i.classList.add('is-invalid');
        this.fieldError(form,i.name||i.id,'Please enter a valid email address.');
        ok=false;
      }
    });
    form.querySelectorAll('input[minlength]').forEach(i=>{
      const min=Number(i.getAttribute('minlength'));
      if(i.value && i.value.length<min){
        i.classList.add('is-invalid');
        this.fieldError(form,i.name||i.id,`Minimum ${min} characters required.`);
        ok=false;
      }
    });
    return ok;
  }
};
async function appLogout(){try{const r=await app.request('/Account/LogoutAjax',{method:'POST',body:'{}'});location.href=r.redirectUrl;}catch(e){app.toast(e.message,'error');}}

document.addEventListener('DOMContentLoaded',()=>{
  const page=document.body.dataset.page || document.querySelector('[data-page]')?.dataset.page;
  if(page==='login') initLogin(); if(page==='dashboard') initDashboard(); if(page==='patients') initPatients(); if(page==='patient-form') initPatientForm();
  if(page==='doctors') initDoctors(); if(page==='doctor-form') initDoctorForm(); if(page==='appointments') initAppointments(); if(page==='appointment-form') initAppointmentForm();
  if(page==='prescriptions') initPrescriptions(); if(page==='prescription-form') initPrescriptionForm(); if(page==='reports') initReports(); if(page==='settings') initSettings();
});
function initLogin(){const f=document.getElementById('loginForm');if(!f)return;f.addEventListener('submit',async e=>{e.preventDefault();const err=document.getElementById('loginError');err.textContent='';err.classList.add('d-none');if(!app.validate(f)){return;}const b=f.querySelector('button');app.setBusy(b,true);try{const r=await app.request('/Account/LoginAjax',{method:'POST',body:JSON.stringify({username:f.Username.value.trim(),password:f.Password.value})});location.href=r.redirectUrl;}catch(x){if(x.validationErrors){app.applyServerErrors(f,x);}else{err.textContent=x.message;err.classList.remove('d-none');}}finally{app.setBusy(b,false);}});}
function initDashboard(){loadDashboard();}
async function loadDashboard(){try{const r=await app.request('/Dashboard/Data',{method:'POST',body:'{}'});const d=r.data;document.getElementById('statPatients').textContent=d.patients;document.getElementById('statDoctors').textContent=d.doctors;document.getElementById('statAppointments').textContent=d.todayAppointments;document.getElementById('statPrescriptions').textContent=d.pendingPrescriptions;document.getElementById('dashboardRows').innerHTML=d.appointments.map(a=>`<tr><td><b>${app.escape(a.appointmentTime)}</b></td><td>${app.escape(a.patientName)}</td><td>${app.escape(a.doctorName)}</td><td>${app.escape(a.reason)}</td><td><span class="badge-soft">${app.escape(a.status)}</span></td></tr>`).join('')||'<tr><td colspan="5" class="text-center py-4">No appointments scheduled for today.</td></tr>';}catch(e){app.toast(e.message,'error');}}
async function initPatients(){const search=document.getElementById('patientSearch');document.getElementById('patientSearchForm')?.addEventListener('submit',e=>{e.preventDefault();loadPatients(search.value);});loadPatients('');}
async function loadPatients(search){try{const r=await app.request('/Patients/List',{method:'POST',body:JSON.stringify({search})});document.getElementById('patientRows').innerHTML=(r.data||[]).map(p=>`<tr><td>${p.id}</td><td><b>${app.escape(p.patientNumber)}</b></td><td>${app.escape(p.firstName)} ${app.escape(p.lastName)}</td><td>${app.escape(p.gender)}</td><td>${app.escape(p.phone)}</td><td>${new Date(p.dateOfBirth).toLocaleDateString('en-GB')}</td><td><span class="badge-soft">${app.escape(p.status)}</span></td><td><a class="btn btn-sm btn-light me-1" href="/Patients/Edit/${p.id}"><i class="bi bi-pencil"></i></a><button class="btn btn-sm btn-light" onclick="deletePatient(${p.id})"><i class="bi bi-trash"></i></button></td></tr>`).join('')||'<tr><td colspan="8" class="text-center py-4">No patients found.</td></tr>';}catch(e){app.toast(e.message,'error');}}
async function deletePatient(id){if(!confirm('Delete this patient? This cannot be undone.'))return;try{const r=await app.request('/Patients/Delete',{method:'POST',body:JSON.stringify({id})});app.toast(r.message);loadPatients(document.getElementById('patientSearch').value);}catch(e){app.toast(e.message,'error');}}
async function initPatientForm(){const f=document.getElementById('patientForm');const id=Number(f.dataset.id||0);if(id){try{const r=await app.request(`/Patients/Get/${id}`);fillForm(f,r.data);}catch(e){app.toast(e.message,'error');}}f.addEventListener('submit',async e=>{e.preventDefault();if(!app.validate(f))return;const b=f.querySelector('button');app.setBusy(b,true);try{const r=await app.request('/Patients/Save',{method:'POST',body:JSON.stringify(formObject(f))});app.toast(r.message);setTimeout(()=>location.href='/Patients',500);}catch(x){app.applyServerErrors(f,x);}finally{app.setBusy(b,false);}});}
async function initDoctors(){loadDoctors();}
async function loadDoctors(){try{const r=await app.request('/Doctors/List',{method:'POST',body:'{}'});document.getElementById('doctorGrid').innerHTML=(r.data||[]).map(d=>`<div class="doctor-card"><div class="doctor-avatar"><i class="bi bi-person"></i></div><h3>${app.escape(d.name)}</h3><div class="speciality">${app.escape(d.specialization)}</div><div class="department">${app.escape(d.department)}</div><hr/><div class="doctor-meta"><span><i class="bi bi-award"></i> ${d.experienceYears} years</span><span><i class="bi bi-telephone"></i> ${app.escape(d.phone)}</span></div><div class="mt-3"><a class="btn btn-sm btn-light me-1" href="/Doctors/Edit/${d.id}">Edit</a><button class="btn btn-sm btn-light" onclick="deleteDoctor(${d.id})">Delete</button></div></div>`).join('')||'<div class="empty-state">No doctors found.</div>';}catch(e){app.toast(e.message,'error');}}
async function deleteDoctor(id){if(!confirm('Delete this doctor?'))return;try{const r=await app.request('/Doctors/Delete',{method:'POST',body:JSON.stringify({id})});app.toast(r.message);loadDoctors();}catch(e){app.toast(e.message,'error');}}
async function initDoctorForm(){const f=document.getElementById('doctorForm');const id=Number(f.dataset.id||0);if(id){try{const r=await app.request(`/Doctors/Get/${id}`);fillForm(f,r.data);}catch(e){app.toast(e.message,'error');}}f.addEventListener('submit',async e=>{e.preventDefault();if(!app.validate(f))return;const b=f.querySelector('button');app.setBusy(b,true);try{const r=await app.request('/Doctors/Save',{method:'POST',body:JSON.stringify(formObject(f))});app.toast(r.message);setTimeout(()=>location.href='/Doctors',500);}catch(x){app.applyServerErrors(f,x);}finally{app.setBusy(b,false);}});}
async function initAppointments(){const f=document.getElementById('appointmentSearchForm');f?.addEventListener('submit',e=>{e.preventDefault();loadAppointments();});document.getElementById('appointmentSearch')?.addEventListener('input',()=>loadAppointments());loadAppointments();}
async function loadAppointments(){try{const r=await app.request('/Appointments/List',{method:'POST',body:JSON.stringify({search:document.getElementById('appointmentSearch')?.value||'',date:document.getElementById('appointmentDateFilter')?.value||''})});document.getElementById('appointmentRows').innerHTML=(r.data||[]).map(a=>`<tr><td>${new Date(a.appointmentDate).toLocaleDateString('en-GB')}</td><td>${app.escape(a.appointmentTime)}</td><td>${app.escape(a.patientName)}</td><td>${app.escape(a.doctorName)}</td><td>${app.escape(a.reason)}</td><td><select class="form-select form-select-sm" onchange="changeAppointmentStatus(${a.id},this.value)"><option ${a.status==='Confirmed'?'selected':''}>Confirmed</option><option ${a.status==='Waiting'?'selected':''}>Waiting</option><option ${a.status==='Completed'?'selected':''}>Completed</option><option ${a.status==='Cancelled'?'selected':''}>Cancelled</option></select></td><td><a class="btn btn-sm btn-light me-1" href="/Appointments/Edit/${a.id}">Edit</a><button class="btn btn-sm btn-light" onclick="deleteAppointment(${a.id})">Delete</button></td></tr>`).join('')||'<tr><td colspan="7" class="text-center py-4">No appointments found.</td></tr>';}catch(e){app.toast(e.message,'error');}}
async function changeAppointmentStatus(id,status){try{const r=await app.request('/Appointments/Status',{method:'POST',body:JSON.stringify({id,status})});app.toast(r.message);}catch(e){app.toast(e.message,'error');loadAppointments();}}
async function deleteAppointment(id){if(!confirm('Delete this appointment?'))return;try{const r=await app.request('/Appointments/Delete',{method:'POST',body:JSON.stringify({id})});app.toast(r.message);loadAppointments();}catch(e){app.toast(e.message,'error');}}
async function initAppointmentForm(){const f=document.getElementById('appointmentForm');await fillAppointmentLookups();const id=Number(f.dataset.id||0);if(id){try{const r=await app.request(`/Appointments/Get/${id}`);fillForm(f,r.data);}catch(e){app.toast(e.message,'error');}}f.addEventListener('submit',async e=>{e.preventDefault();if(!app.validate(f))return;const b=f.querySelector('button');app.setBusy(b,true);try{const r=await app.request('/Appointments/Save',{method:'POST',body:JSON.stringify(formObject(f))});app.toast(r.message);setTimeout(()=>location.href='/Appointments',500);}catch(x){app.applyServerErrors(f,x);}finally{app.setBusy(b,false);}});}
async function fillAppointmentLookups(){try{const [p,d]=await Promise.all([app.request('/Patients/List',{method:'POST',body:JSON.stringify({search:''})}),app.request('/Doctors/List',{method:'POST',body:'{}'})]);document.getElementById('PatientId').innerHTML=(p.data||[]).map(x=>`<option value="${x.id}">${app.escape(x.patientNumber)} - ${app.escape(x.firstName)} ${app.escape(x.lastName)}</option>`).join('');document.getElementById('DoctorId').innerHTML=(d.data||[]).map(x=>`<option value="${x.id}">${app.escape(x.name)} (${app.escape(x.specialization)})</option>`).join('');}catch(e){app.toast(e.message,'error');}}
async function initPrescriptions(){document.getElementById('prescriptionSearchForm')?.addEventListener('submit',e=>{e.preventDefault();loadPrescriptions();});loadPrescriptions();}
async function loadPrescriptions(){try{const r=await app.request('/Prescriptions/List',{method:'POST',body:JSON.stringify({search:document.getElementById('prescriptionSearch')?.value||''})});document.getElementById('prescriptionRows').innerHTML=(r.data||[]).map(p=>`<tr><td>${new Date(p.createdAt).toLocaleDateString('en-GB')}</td><td>${app.escape(p.patient.firstName)} ${app.escape(p.patient.lastName)}</td><td>${app.escape(p.doctor.name)}</td><td>${app.escape(p.diagnosis)}</td><td><span class="badge-soft">${p.medicines.length} medicine(s)</span></td><td><a class="btn btn-sm btn-light me-1" href="/Prescriptions/Edit/${p.id}">Edit</a><button class="btn btn-sm btn-light" onclick="deletePrescription(${p.id})">Delete</button></td></tr>`).join('')||'<tr><td colspan="6" class="text-center py-4">No prescriptions found.</td></tr>';}catch(e){app.toast(e.message,'error');}}
async function deletePrescription(id){if(!confirm('Delete this prescription?'))return;try{const r=await app.request('/Prescriptions/Delete',{method:'POST',body:JSON.stringify({id})});app.toast(r.message);loadPrescriptions();}catch(e){app.toast(e.message,'error');}}
async function initPrescriptionForm(){const f=document.getElementById('prescriptionForm');const id=Number(f.dataset.id||0);try{const [p,d,a]=await Promise.all([app.request('/Patients/List',{method:'POST',body:JSON.stringify({search:''})}),app.request('/Doctors/List',{method:'POST',body:'{}'}),app.request('/Appointments/List',{method:'POST',body:JSON.stringify({search:'',date:''})})]);document.getElementById('PatientId').innerHTML=p.data.map(x=>`<option value="${x.id}">${app.escape(x.patientNumber)} - ${app.escape(x.firstName)} ${app.escape(x.lastName)}</option>`).join('');document.getElementById('DoctorId').innerHTML=d.data.map(x=>`<option value="${x.id}">${app.escape(x.name)}</option>`).join('');document.getElementById('AppointmentId').innerHTML=a.data.map(x=>`<option value="${x.id}">${new Date(x.appointmentDate).toLocaleDateString('en-GB')} - ${app.escape(x.patientName)} / ${app.escape(x.doctorName)}</option>`).join('');if(id){const x=await app.request(`/Prescriptions/Get/${id}`);fillForm(f,x.data);if(x.data.medicines?.[0]){f.MedicineName.value=x.data.medicines[0].medicineName;f.Dosage.value=x.data.medicines[0].dosage;f.Frequency.value=x.data.medicines[0].frequency;f.Duration.value=x.data.medicines[0].duration;f.Instructions.value=x.data.medicines[0].instructions;}}}catch(e){app.toast(e.message,'error');}
f.addEventListener('submit',async e=>{e.preventDefault();if(!app.validate(f))return;const b=f.querySelector('button');app.setBusy(b,true);const medicine={medicineName:f.MedicineName.value.trim(),dosage:f.Dosage.value.trim(),frequency:f.Frequency.value.trim(),duration:f.Duration.value.trim(),instructions:f.Instructions.value.trim()};try{const r=await app.request('/Prescriptions/Save',{method:'POST',body:JSON.stringify({id,patientId:+f.PatientId.value,doctorId:+f.DoctorId.value,appointmentId:+f.AppointmentId.value,diagnosis:f.Diagnosis.value.trim(),notes:f.Notes.value.trim(),medicines:[medicine]})});app.toast(r.message);setTimeout(()=>location.href='/Prescriptions',500);}catch(x){app.applyServerErrors(f,x);}finally{app.setBusy(b,false);}});}
async function initReports(){document.getElementById('reportForm').addEventListener('submit',async e=>{e.preventDefault();const f=e.target;if(!app.validate(f))return;try{const r=await app.request('/Reports/Summary',{method:'POST',body:JSON.stringify({from:f.From.value,to:f.To.value})});const d=r.data;document.getElementById('reportCards').innerHTML=`<div class="stat-card blue"><span class="stat-icon"><i class="bi bi-people"></i></span><div><small>New Patients</small><strong>${d.totalPatients}</strong></div></div><div class="stat-card green"><span class="stat-icon"><i class="bi bi-calendar-check"></i></span><div><small>Appointments</small><strong>${d.appointments}</strong></div></div><div class="stat-card orange"><span class="stat-icon"><i class="bi bi-check2-circle"></i></span><div><small>Completed</small><strong>${d.completedAppointments}</strong></div></div><div class="stat-card purple"><span class="stat-icon"><i class="bi bi-capsule"></i></span><div><small>Prescriptions</small><strong>${d.prescriptions}</strong></div></div>`;document.getElementById('reportMeta').textContent=`Cancelled appointments: ${d.cancelledAppointments} • Total doctors: ${d.totalDoctors}`;}catch(x){app.toast(x.message,'error');}});document.getElementById('reportForm').requestSubmit();}
async function initSettings(){try{const r=await app.request('/Settings/Load',{method:'POST',body:'{}'});const d=r.data;fillForm(document.getElementById('settingsForm'),d);fillForm(document.getElementById('profileForm'),d);}catch(e){app.toast(e.message,'error');}document.getElementById('settingsForm').addEventListener('submit',saveSettings);document.getElementById('profileForm').addEventListener('submit',saveProfile);document.getElementById('passwordForm').addEventListener('submit',changePassword);}
async function saveSettings(e){e.preventDefault();const f=e.target;if(!app.validate(f))return;try{const r=await app.request('/Settings/Save',{method:'POST',body:JSON.stringify(formObject(f))});app.toast(r.message);}catch(x){app.applyServerErrors(f,x);}}
async function saveProfile(e){e.preventDefault();const f=e.target;if(!app.validate(f))return;try{const r=await app.request('/Settings/Profile',{method:'POST',body:JSON.stringify(formObject(f))});app.toast(r.message);setTimeout(()=>location.reload(),500);}catch(x){app.applyServerErrors(f,x);}}
async function changePassword(e){e.preventDefault();const f=e.target;if(!app.validate(f))return;if(f.NewPassword.value!==f.ConfirmPassword.value){app.fieldError(f,'ConfirmPassword','Passwords do not match.');return;}try{const r=await app.request('/Settings/Password',{method:'POST',body:JSON.stringify(formObject(f))});app.toast(r.message);f.reset();}catch(x){app.applyServerErrors(f,x);}}
function formObject(form){const o={};new FormData(form).forEach((v,k)=>{o[k]=v;});if(form.dataset.id)o.Id=Number(form.dataset.id);return o;}
function fillForm(form,data){if(!form||!data)return;const fields=[...form.querySelectorAll('[name],#PatientId,#DoctorId,#AppointmentId')];Object.keys(data).forEach(k=>{const el=fields.find(x=>(x.name||x.id).toLowerCase()===k.toLowerCase());if(el){let v=data[k];if(el.type==='date'&&v)v=String(v).slice(0,10);el.value=v??'';}});}


document.addEventListener('DOMContentLoaded',()=>{
  const path=window.location.pathname.toLowerCase();
  document.querySelectorAll('.sidebar-nav .nav-item').forEach(a=>{
    const href=(a.getAttribute('href')||'').toLowerCase();
    if(href && path.startsWith(href) && href!=='/') a.classList.add('active');
  });
});
