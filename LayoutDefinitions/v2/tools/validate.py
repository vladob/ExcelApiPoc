"""Offline schema + semantic validation. Requires jsonschema==4.26.0."""
import argparse,copy,hashlib,json,math,zipfile
from pathlib import Path
from jsonschema import Draft202012Validator
from referencing import Registry,Resource
R=Path(__file__).resolve().parents[1]
def load(p):return json.loads(p.read_text(encoding='utf-8'),parse_constant=lambda x:(_ for _ in ()).throw(ValueError('Nonfinite JSON '+x)))
def walk(x):
 yield x
 if isinstance(x,dict):
  for v in x.values():yield from walk(v)
 elif isinstance(x,list):
  for v in x:yield from walk(v)
def unique(items,label,errors):
 ids=[x['id'] for x in items]
 if len(ids)!=len(set(ids)):errors.append(label+': duplicate IDs')
def check(root=R,mutate=None,capabilities=None):
 errors=[];schemas={p.name:load(p) for p in (root/'schemas').glob('*.json')}
 for s in schemas.values():Draft202012Validator.check_schema(s)
 registry=Registry().with_resources((s['$id'],Resource.from_contents(s)) for s in schemas.values())
 catalog=load(root/'catalog.json');docs={}
 for item in catalog['definitions']:
  path=(root/item['path']).resolve()
  if not path.is_relative_to(root.resolve()):errors.append('Unsafe catalog path');continue
  d=load(path)
  if hashlib.sha256(path.read_bytes().replace(b'\r\n', b'\n')).hexdigest()!=item['sha256']:errors.append(str(path)+': hash mismatch')
  if item['ref']!={'id':d['id'],'version':d['definitionVersion']}:errors.append(str(path)+': version mismatch')
  if d['id'] in docs:errors.append('Duplicate definition ID '+d['id'])
  docs[d['id']]=d
 if mutate:mutate(docs)
 for d in [catalog,*docs.values()]:
  s=schemas[d['kind']+'.schema.json'];v=Draft202012Validator(s,registry=registry)
  errors.extend(d['id']+': '+ '/'.join(map(str,e.absolute_path))+' '+e.message for e in v.iter_errors(d))
  if capabilities is not None:
   errors.extend(d['id']+': unsupported capability '+c for c in d['requiresCapabilities'] if c not in capabilities)
  for x in walk(d):
   if not isinstance(x,dict):continue
   if set(x)=={'id','version'}:
    if x['id'] not in docs or docs[x['id']]['definitionVersion']!=x['version']:errors.append(d['id']+': unresolved definition '+str(x))
   for key in ['rectPt','offsetRectPt','regionFraction']:
    if key in x:
     a,b,c,e=x[key]
     if not all(math.isfinite(t) for t in [a,b,c,e]) or a>=c or b>=e:errors.append(d['id']+': invalid rectangle '+str(x[key]))
   if 'scaleRange' in x and x['scaleRange'][0]>=x['scaleRange'][1]:errors.append(d['id']+': reversed scale bounds')
  expected_kinds={'modelRef':'model','layoutRef':'layout','parserRefs':'parsers','validationRefs':'validation','identificationRefs':'identification'}
  for key,kind in expected_kinds.items():
   refs=d.get(key,[]);refs=refs if isinstance(refs,list) else [refs]
   for reference in refs:
    if reference['id'] in docs and docs[reference['id']]['kind']!=kind:errors.append(d['id']+': wrong definition kind '+key)
  if d['kind']=='parsers':unique(d['profiles'],d['id'],errors)
  if d['kind']=='model':unique(d['fields'],d['id'],errors)
  if d['kind']=='layout':
   unique(d['variants'],d['id']+' variants',errors);unique(d['classification'],d['id']+' classification',errors);unique(d['groups'],d['id']+' groups',errors);unique(d['metadata'],d['id']+' metadata',errors);unique(d['completeness'],d['id']+' completeness',errors)
   model=docs.get(d['modelRef']['id'],{});names={x['id'] for x in model.get('fields',[])};blockids={b['id'] for v in d['variants'] for b in v['blocks']};groups={g['id']:g for g in d['groups']}
   parserids={p['id'] for r in d['parserRefs'] for p in docs.get(r['id'],{}).get('profiles',[])}
   for v in d['variants']:
    unique(v['blocks'],d['id']+' blocks',errors);unique(v['anchors'],d['id']+' anchors',errors);aids={a['id'] for a in v['anchors']}
    for a in v['anchors']:
     b=next((b for b in v['blocks'] if b['id']==a['blockRef']),None);line=next((x for x in b['lines'] if x['id']==a['lineRef']),None) if b else None
     if line is None or line['pointsPt'][a['endpointIndex']]!=a['pointPt']:errors.append(d['id']+': unresolved anchor ruling')
    if not set(v['calibration']['anchorRefs'])<=aids:errors.append(d['id']+': unknown anchor')
    if v['calibration']['minimumInliers']>len(set(v['calibration']['anchorRefs'])):errors.append(d['id']+': insufficient anchors')
    for b in v['blocks']:
     for key in ['fields','labels','lines']:unique(b[key],d['id']+'/'+v['id']+'/'+b['id']+'/'+key,errors)
     for f in b['fields']:
      if f['parserRef'] not in parserids:errors.append(d['id']+': unknown parser '+f['parserRef'])
      if f['semanticName'] not in names:errors.append(d['id']+': unknown model field')
   for m in d['metadata']:
    if m['parserRef'] not in parserids:errors.append(d['id']+': unknown metadata parser')
    loc=m['locator'];v=next((v for v in d['variants'] if v['id']==loc.get('variantRef')),None)
    if loc['kind']=='blockField':
     b=next((b for b in v['blocks'] if b['id']==loc['blockRef']),None) if v else None
     if not b or loc['fieldRef'] not in {f['id'] for f in b['fields']}:errors.append(d['id']+': unresolved metadata locator')
     elif loc['frame']!=b['frame']:errors.append(d['id']+': metadata frame mismatch')
   metadata_names={m['semanticName'] for m in d['metadata']}
   for derivation in d['metadataDerivations']:
    if not set(derivation['inputMetadataNames'])<=metadata_names:errors.append(d['id']+': unresolved metadata derivation')
   memberships={n for c in d['classification'] for n in c['aggregationMemberships']}
   classes={c['semanticClass'] for c in d['classification']}
   for x in walk([d['groups'],d['completeness']]):
    if isinstance(x,dict) and 'semanticClasses' in x:
     if not set(x['semanticClasses'])<=classes:errors.append(d['id']+': unknown class selector')
     if not set(x['aggregationMemberships'])<=memberships:errors.append(d['id']+': unknown membership selector')
   for part in [d['classification'],d['groups'],d['completeness']]:
    for x in walk(part):
     if not isinstance(x,dict):continue
     for f in x.get('fieldRefs',[])+x.get('keyFields',[]):
      if f not in names:errors.append(d['id']+': unknown semantic field '+f)
     if 'fieldRef'in x and x['fieldRef'] not in names:errors.append(d['id']+': unknown semantic field '+x['fieldRef'])
     if not set(x.get('blockRefs',[]))<=blockids:errors.append(d['id']+': unknown block')
   for g in groups.values():
    seen={g['id']};parent=g['parentRef']
    while parent is not None:
     if parent not in groups:errors.append(d['id']+': unknown parent '+parent);break
     if parent in seen:errors.append(d['id']+': group cycle');break
     seen.add(parent);parent=groups[parent]['parentRef']
  if d['kind']=='identification':
   unique(d['signals'],d['id']+' signals',errors);signals={s['id']:s for s in d['signals']};eg={g['id'] for g in d['evidenceGroups']}
   for s in signals.values():
    if s['evidenceGroup'] not in eg:errors.append(d['id']+': unknown evidence group')
    if s['op']=='geometryFit':
     target=docs.get(s['layoutRef']['id'],{})
     if not set(s['variantRefs'])<={v['id'] for v in target.get('variants',[])}:errors.append(d['id']+': unresolved geometry variant')
   for decision in d['decisions']:
    if not set(decision['requiredSignals']+decision['vetoSignals'])<=signals.keys():errors.append(d['id']+': unknown decision signal')
  if d['kind']=='validation':
   l=docs.get(d['layoutRef']['id'],{});fields={f['semanticName'] for v in l.get('variants',[]) for b in v['blocks'] for f in b['fields']};groupids={g['id'] for g in l.get('groups',[])}
   for key in ['printedControls','calculations','rules']:unique(d[key],d['id']+'/'+key,errors)
   calcs={c['id']:c for c in d['calculations']};controls={c['id'] for c in d['printedControls']}
   for c in d['printedControls']:
    v=next((v for v in l.get('variants',[]) if v['id']==c['variantRef']),None);b=next((b for b in v['blocks'] if b['id']==c['blockRef']),None) if v else None
    if not b or c['fieldRef'] not in {f['id'] for f in b['fields']}:errors.append(d['id']+': unresolved printed control')
   memberships={n for c in l.get('classification',[]) for n in c['aggregationMemberships']}
   classes={c['semanticClass'] for c in l.get('classification',[])}
   for c in d['calculations']:
    if not set(c['memberSelector']['aggregationMemberships'])<=memberships:errors.append(d['id']+': unknown calculation membership')
    if not set(c['memberSelector']['semanticClasses'])<=classes:errors.append(d['id']+': unknown calculation class')
    for inp in c['inputRefs']:
     if inp['kind']=='field' and inp['semanticName'] not in fields:errors.append(d['id']+': unknown calculation field')
     if inp['kind']=='calculation' and inp['ruleRef'] not in calcs:errors.append(d['id']+': unknown calculation reference')
   def visit(cid,ancestors):
    if cid in ancestors:errors.append(d['id']+': calculation cycle');return
    for x in calcs[cid]['inputRefs']:
     if x['kind']=='calculation' and x['ruleRef'] in calcs:visit(x['ruleRef'],ancestors|{cid})
   for cid in calcs:visit(cid,set())
   for c in d['rules']:
    if c['op']=='compare' and (c['calculatedRef'] not in calcs or (c['printedControlRef'] not in controls if 'printedControlRef' in c else c.get('otherCalculatedRef') not in calcs)):errors.append(d['id']+': unresolved comparison')
   for x in walk(d):
    if isinstance(x,dict) and x.get('kind')=='group' and x.get('groupRef') not in groupids:errors.append(d['id']+': unresolved group scope')
 return errors,docs

def preserved(docs):
 z=zipfile.ZipFile(R/'sources/IfoSoft-v2-review.zip');count=0;errors=[]
 for name in z.namelist():
  if '/layouts/' not in name or 'account-plan' in name:continue
  old=json.loads(z.read(name));new=docs[old['id']]
  for ov,nv in zip(old['variants'],new['variants'],strict=True):
   for ob,nb in zip(ov['blocks'],nv['blocks'],strict=True):
    for kind,key in [('fields','rectPt'),('labels','rectPt'),('lines','pointsPt')]:
     for a,b in zip(ob.get(kind,[]),nb[kind],strict=True):
      count+=1
      if a[key]!=b[key] or a['id']!=b['id']:errors.append(name+': geometry changed')
 return count,errors

def main():
 a=argparse.ArgumentParser();a.add_argument('--self-test',action='store_true');args=a.parse_args()
 errors,docs=check();count,geo=preserved(docs);errors+=geo
 if errors:
  print('\n'.join(errors[:30]));raise SystemExit(1)
 tests=[]
 if args.self_test:
  lid='ifosoft.journal-dennik1'
  mutations={
   'unknown executable property':lambda d:d[lid]['variants'][0]['calibration'].update(typo=1),
   'unknown operation':lambda d:d[lid]['completeness'][0].update(op='executeCode'),
   'missing reference':lambda d:d[lid]['modelRef'].update(id='missing'),
   'wrong version':lambda d:d[lid]['modelRef'].update(version='0.0.0'),
   'duplicate IDs':lambda d:d[lid]['variants'][0]['blocks'].append(copy.deepcopy(d[lid]['variants'][0]['blocks'][0])),
   'unknown parser':lambda d:d[lid]['variants'][0]['blocks'][0]['fields'][0].update(parserRef='missing'),
   'reversed rectangle':lambda d:d[lid]['variants'][0]['blocks'][0]['fields'][0].update(rectPt=[10,0,0,20]),
   'unknown anchor':lambda d:d[lid]['variants'][0]['calibration']['anchorRefs'].append('missing'),
   'group cycle':lambda d:d[lid]['groups'][0].update(parentRef='reportSection'),
   'calculation cycle':lambda d:d['ifosoft.general-ledger-hlknia4.validation']['calculations'].append(dict(d['ifosoft.general-ledger-hlknia4.validation']['calculations'][-1],id='cycle',inputRefs=[{'kind':'calculation','ruleRef':'cycle'},{'kind':'literal','decimal':'0'}])),
   'unknown metadata field':lambda d:d[lid]['metadata'][0]['locator'].update(fieldRef='missing')}
  for name,mutation in mutations.items():
   found,_=check(mutate=mutation)
   if not found:raise AssertionError('Accepted invalid fixture: '+name)
   tests.append(name)
  blocked,_=check(capabilities=set())
  assert any('unsupported capability' in e for e in blocked);tests.append('unsupported capabilities')
 report={'schemaValidation':'pass','referenceValidation':'pass','geometryPreservedItems':count,'definitions':len(docs),'layouts':sum(d['kind']=='layout' for d in docs.values()),'negativeCasesPassed':tests,'engineExecution':'notImplementedInStep2','corpusExecution':'notRun'}
 (R/'verification.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8',newline='\n');print(json.dumps(report,indent=2))
if __name__=='__main__':main()
